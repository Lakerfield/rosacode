# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Lakerfield.RosaCode embeds the Monaco editor in WPF (via WebView2) and backs it with Roslyn for C# IntelliSense. The editor UI and the language engine are decoupled through the `IRosaCodeEngine` interface, so the engine can run in-process or remotely (RPC). `plan.md` lists open gaps and improvements with priorities.

## Commands

- Build: `dotnet build rosacode.sln` (WPF projects target `net8.0-windows`; on non-Windows add `-p:EnableWindowsTargeting=true`). A running Playground locks the DLLs: close it before building the WPF projects.
- Run the demo: `dotnet run --project src/Lakerfield.RosaCode.Playground` (in-process Roslyn engine)
- RPC demo: start `src/RpcDemo.ServerApp` (WebSocket server at `/ws`), then `src/RpcDemo.Playground`
- There are no test projects (CI runs `dotnet test`, which finds nothing). Engine behaviour was checked with throwaway console apps that reference `Lakerfield.RosaCode.Roslyn` and call the engine directly (`new RosaCodeRoslynEngine(new RosaCodeRoslynEngineOptions { LoadXmlDocumentation = false })`).
- Testing the editor page without WPF: run `Resources/RosaCodeEditor.html` in headless Edge with a fake `window.chrome.webview` (records `postMessage`, answers with canned JSON through the `message` listener). Use `msedge --headless=new --virtual-time-budget=… --dump-dom`; wait for the C# grammar with `await monaco.languages.getLanguages().find(l => l.id === "csharp").loader()` and poll for DOM changes instead of fixed sleeps, otherwise results are flaky.
- Releases: pushing a `vX.Y.Z*` tag triggers `.github/workflows/build-and-publish.yml` (GitVersion → pack `Lakerfield.RosaCode`, `.Controls`, `.Roslyn` → NuGet push). The Playground and RpcDemo* projects are not packaged.

## Architecture

Project dependency flow: `Lakerfield.RosaCode` (contracts) ← `.Controls` (WPF editor) and `.Roslyn` (engine). The Playground apps wire a Controls editor to an engine.

### `Lakerfield.RosaCode` (contracts)

- `IRosaCodeEngine`: the contract, also used over RPC, so **no `CancellationToken` in it** (the RPC generator turns every parameter into a serialized message field).
- `ICancellableRosaCodeEngine : IRosaCodeEngine`: the same methods with a token. The editor prefers it when the engine implements it; remote engines only implement the base interface.
- `Models.cs`: the DTOs. Keep them **flat** (no recursive types: the BSON layer cannot map them; the outline is a flat list with `ParentId`). Keep the project dependency-free.

### `Lakerfield.RosaCode.Controls` (WPF)

- `RosaCodeEditor` UserControl hosts a WebView2. `Resources/RosaCodeEditor.html` (Monaco + JS glue, embedded resource, loaded with `NavigateToString`) talks to C# with JSON `WebMessage { Id, Method, Json }`. JSON is camelCase; the `code` field is a JSON string inside JSON (`Deserialize<string>(request.Code)`).
- Page → C#: request/response calls have `Id > 0` and always get an answer (`Reply`, or an `"error"` message that rejects the call in JS). `Id == 0` are notifications (`ready`, `textChanged`, `cancel`). `WebViewWebMessageReceived` → `HandleWebMessage` dispatches on `Method`: `completion`, `completionResolve`, `hover`, `signatures`, `diagnostics`, `format`, `formatRange`, `action`, `definition`, `references`, `renameInfo`, `rename`, `symbols`, `semanticTokens`, `inlayHints`, `folding`.
- C# → page: `PostWebMessage(-1, method, json)`. Everything the host sets before the editor is ready (`Mode`, `SetTheme`, `SetOptions`, `SetDiagnosticsDelay`, `SetOriginalCode`, text) is queued and flushed on `ready`, in that order, theme before code.
- Cancellation: each request has a `CancellationTokenSource` in `_runningRequests`; the page sends `cancel {id}` when Monaco cancels (and when the text changes during diagnostics). A late answer for a cancelled call is ignored by the page.
- `Text` is a two-way dependency property kept current by `textChanged`; `GetCode()` returns the editor's text, not the engine's. `InitializeEditor(engine)` can be called only once; `CleanupEditor()` disposes the WebView.
- Adding a feature touches: the contract (both interfaces), the DTOs, an engine partial, the C# switch in the control + a helper using `CallEngine`, the RPC server forwarder, the BSON class maps, and a provider in the HTML.

### `Lakerfield.RosaCode.Roslyn` (engine)

`RosaCodeRoslynEngine` is a partial class over an `AdhocWorkspace` with a single document. Public methods exist with and without a `CancellationToken` and run through `RunExclusive` (one request at a time; a request cancelled while waiting never runs). Monaco line/column are 1-based; `GetPosition` clamps them. Files:

- `RosaCodeRoslynEngine.cs`: constructor, project setup, `RunExclusive`, `UpdateCode`, code actions (`GetActions`, edits mapped to Monaco ranges with `CreateRange`/`CreateEdit`).
- `.Completions` (lazy documentation via `GetCompletionDescription`, `override`/`partial` items with their full text and replace range, commit characters limited to `.` and `(`), `.Hover`, `.Signatures` (all overloads, named arguments), `.Formatting` (document and range; only whitespace changes), `.Diagnostics` (document only; unused usings come as hidden + "Unnecessary"), `.Navigation` (definition, references, rename, outline), `.SemanticTokens` (Roslyn classifier), `.InlayHints`, `.Folding` (syntax only), `.References` (per-engine references, `AddReferences`, background documentation load, the generated `.editorconfig`).
- `RoslynCodeActionProvider`: runs Roslyn's own `CodeFixProvider`/`CodeRefactoringProvider` MEF exports from one shared container that the workspace also uses. Actions that need UI services (e.g. "Generate type" with options) are skipped.
- `RosaCodeRoslynEngineOptions`: per engine `Assemblies`/`References`/`AllowedReferencePrefixes`, `LoadXmlDocumentation`, `IndentSize`/`UseTabs`. `RosaCodeRoslynConstants` holds only the global defaults for the prefix filter.
- Generated code follows the editor's indentation through an `.editorconfig` analyzer-config document in the workspace (workspace options have no effect in this Roslyn version).
- The framework XML documentation is used from the AppData cache (`%APPDATA%/RosaCode/<runtime version>/ref/...`) when present, otherwise `XmlHelper` downloads it in the background (`DocumentationLoaded`) and the references are swapped in; offline the engine works without it.
- `GenericImplHelper`, `UsingHelper`, `MapCodeActionHelper` and `MetadataReferenceGenerator` are unused leftovers.

### RpcDemo\*

Shows a remote engine. `IRpcRosaCodeEngine : IRosaCodeEngine` is marked `[RpcService]`; the `Lakerfield.Rpc.SourceGenerator` generates the client/server stubs (committed under `Generated/`, rewritten on build). `MyRosaCodeEngineServer` forwards every interface method by hand and must get each new method. BSON class maps for every DTO must be registered in `RpcRosaCodeEngineBsonConfigurator`.

## Editor page notes

- Monaco 0.57.0 from cdnjs with the AMD loader (deprecated upstream, see `plan.md` 2.1). The version appears twice in the HTML (script `src` and `MONACO_PATH`).
- Themes `vs`/`vs-dark` are mapped to `rosa-light`/`rosa-dark`, which add colours for semantic token types. Monaco keeps the colours of semantic tokens of the previous theme until the next edit: set the theme before the code.
- Defaults in `_editorOptions`: `formatOnType`, `formatOnPaste`, `semanticHighlighting.enabled`. Inlay hints and colour swatches (hex strings such as `"#FF8800"`, `#AARRGGBB` order) are on through Monaco's own defaults; hosts turn them off with `SetOptions`.
- Providers must not throw into Monaco: wrap them in `safely(...)`, pass the Monaco token to `callWpf(method, data, token)` so cancelling works.

## Conventions

- NuGet versions are centralized in `Directory.Build.targets` through `PackageReference Update=`; csproj files reference packages without versions. Shared package metadata is in `Directory.Build.props`.
- Code uses 2-space indentation with braces on their own line (see `.editorconfig`). Source files mix CRLF and LF; keep the line endings of the file you edit.
