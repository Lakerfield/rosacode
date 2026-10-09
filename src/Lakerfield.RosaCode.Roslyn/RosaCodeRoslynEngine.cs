
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using System.Xml.Linq;

using static Basic.Reference.Assemblies.Net80.References;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine : ICancellableRosaCodeEngine
  {
    private object _lock = new object();
    private AdhocWorkspace _workspace;
    private Document _document;


    public RosaCodeRoslynEngine(string name = "default", string code = "")
      : this(new RosaCodeRoslynEngineOptions { Name = name, Code = code })
    {
    }

    public RosaCodeRoslynEngine(RosaCodeRoslynEngineOptions options)
    {
      var name = options.Name;
      var code = options.Code ?? string.Empty;

      _workspace = new AdhocWorkspace(RoslynCodeActionProvider.Host);
      Task.Run(RoslynCodeActionProvider.Preload); // first use takes about a second

      //var metadataReferences = GenerateMetadataReferences();

      var refs = new[] {
            MicrosoftCSharp,
            //MicrosoftVisualBasicCore,
            //MicrosoftVisualBasic,
            MicrosoftWin32Primitives,
            //MicrosoftWin32Registry,
            mscorlib,
            netstandard,
            SystemAppContext,
            SystemBuffers,
            SystemCollectionsConcurrent,
            SystemCollections,
            SystemCollectionsImmutable,
            SystemCollectionsNonGeneric,
            SystemCollectionsSpecialized,
            SystemComponentModelAnnotations,
            SystemComponentModelDataAnnotations,
            SystemComponentModel,
            SystemComponentModelEventBasedAsync,
            SystemComponentModelPrimitives,
            SystemComponentModelTypeConverter,
            SystemConfiguration,
            SystemConsole,
            SystemCore,
            SystemDataCommon,
            SystemDataDataSetExtensions,
            SystemData,
            SystemDiagnosticsContracts,
            SystemDiagnosticsDebug,
            SystemDiagnosticsDiagnosticSource,
            SystemDiagnosticsFileVersionInfo,
            SystemDiagnosticsProcess,
            SystemDiagnosticsStackTrace,
            SystemDiagnosticsTextWriterTraceListener,
            SystemDiagnosticsTools,
            SystemDiagnosticsTraceSource,
            SystemDiagnosticsTracing,
            Basic.Reference.Assemblies.Net80.References.System,
            //SystemDrawing,
            //SystemDrawingPrimitives,
            //SystemDynamicRuntime,
            //SystemFormatsAsn1,
            SystemGlobalizationCalendars,
            SystemGlobalization,
            SystemGlobalizationExtensions,
            SystemIOCompressionBrotli,
            SystemIOCompression,
            SystemIOCompressionFileSystem,
            SystemIOCompressionZipFile,
            SystemIO,
            SystemIOFileSystemAccessControl,
            SystemIOFileSystem,
            SystemIOFileSystemDriveInfo,
            SystemIOFileSystemPrimitives,
            SystemIOFileSystemWatcher,
            SystemIOIsolatedStorage,
            SystemIOMemoryMappedFiles,
            SystemIOPipesAccessControl,
            SystemIOPipes,
            SystemIOUnmanagedMemoryStream,
            SystemLinq,
            SystemLinqExpressions,
            SystemLinqParallel,
            SystemLinqQueryable,
            SystemMemory,
            SystemNet,
            SystemNetHttp,
            SystemNetHttpJson,
            SystemNetHttpListener,
            SystemNetMail,
            SystemNetNameResolution,
            SystemNetNetworkInformation,
            SystemNetPing,
            SystemNetPrimitives,
            SystemNetRequests,
            SystemNetSecurity,
            SystemNetServicePoint,
            SystemNetSockets,
            SystemNetWebClient,
            SystemNetWebHeaderCollection,
            SystemNetWebProxy,
            SystemNetWebSocketsClient,
            SystemNetWebSockets,
            SystemNumerics,
            SystemNumericsVectors,
            SystemObjectModel,
            SystemReflectionDispatchProxy,
            SystemReflection,
            SystemReflectionEmit,
            SystemReflectionEmitILGeneration,
            SystemReflectionEmitLightweight,
            SystemReflectionExtensions,
            SystemReflectionMetadata,
            SystemReflectionPrimitives,
            SystemReflectionTypeExtensions,
            SystemResourcesReader,
            SystemResourcesResourceManager,
            SystemResourcesWriter,
            SystemRuntimeCompilerServicesUnsafe,
            SystemRuntimeCompilerServicesVisualC,
            SystemRuntime,
            SystemRuntimeExtensions,
            SystemRuntimeHandles,
            SystemRuntimeInteropServices,
            SystemRuntimeInteropServicesRuntimeInformation,
            SystemRuntimeIntrinsics,
            SystemRuntimeLoader,
            SystemRuntimeNumerics,
            SystemRuntimeSerialization,
            SystemRuntimeSerializationFormatters,
            SystemRuntimeSerializationJson,
            SystemRuntimeSerializationPrimitives,
            SystemRuntimeSerializationXml,
            SystemSecurityAccessControl,
            SystemSecurityClaims,
            SystemSecurityCryptographyAlgorithms,
            SystemSecurityCryptographyCng,
            SystemSecurityCryptographyCsp,
            SystemSecurityCryptographyEncoding,
            SystemSecurityCryptographyOpenSsl,
            SystemSecurityCryptographyPrimitives,
            SystemSecurityCryptographyX509Certificates,
            SystemSecurity,
            SystemSecurityPrincipal,
            SystemSecurityPrincipalWindows,
            SystemSecuritySecureString,
            SystemServiceModelWeb,
            SystemServiceProcess,
            SystemTextEncodingCodePages,
            SystemTextEncoding,
            SystemTextEncodingExtensions,
            SystemTextEncodingsWeb,
            SystemTextJson,
            SystemTextRegularExpressions,
            SystemThreadingChannels,
            SystemThreading,
            SystemThreadingOverlapped,
            SystemThreadingTasksDataflow,
            SystemThreadingTasks,
            SystemThreadingTasksExtensions,
            SystemThreadingTasksParallel,
            SystemThreadingThread,
            SystemThreadingThreadPool,
            SystemThreadingTimer,
            SystemTransactions,
            SystemTransactionsLocal,
            SystemValueTuple,
        //SystemWeb,
        //SystemWebHttpUtility,
        //SystemWindows,
            SystemXml,
            SystemXmlLinq,
            SystemXmlReaderWriter,
            SystemXmlSerialization,
            SystemXmlXDocument,
            SystemXmlXmlDocument,
            SystemXmlXmlSerializer,
            SystemXmlXPath,
            SystemXmlXPathXDocument
            //WindowsBase,
          };

      var project = _workspace
      .CurrentSolution
      .AddProject($"{name} Project", name, LanguageNames.CSharp)
      .WithParseOptions(new CSharpParseOptions(documentationMode: DocumentationMode.Parse))
      .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
      //.AddMetadataReferences(refs)
      .AddMetadataReferences(options.LoadXmlDocumentation ? XmlHelper.GetRefs(refs) : refs)
      .AddMetadataReferences(CreateReferences(options))
      //.AddMetadataReferences(metadataReferences)
      ;

      // Roslyn takes the formatting of the code it generates (add using, implement interface, ...) from .editorconfig settings
      // that apply to the path of the document: use the indentation of the editor.
      var directory = "/rosacode";
      project = project.AddAnalyzerConfigDocument(".editorconfig", SourceText.From(CreateEditorConfig(options)), filePath: $"{directory}/.editorconfig").Project;

      _document = project.AddDocument($"{name}.cs", SourceText.From(code), filePath: $"{directory}/{name}.cs");

      _workspace.TryApplyChanges(_document.Project.Solution);

      // Without the documentation on disk the engine starts without it: it is downloaded in the background (see DocumentationLoaded)
      if (options.LoadXmlDocumentation && XmlHelper.TryGetCachedRefPath() == null)
        DocumentationLoaded = Task.Run(() => LoadDocumentationAsync(refs));
    }

    private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

    // All requests share one workspace document, so they run one at a time.
    // A request that is cancelled while it waits for its turn never starts.
    private async Task<T> RunExclusive<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
      await _gate.WaitAsync(cancellationToken);
      try
      {
        cancellationToken.ThrowIfCancellationRequested();
        return await action(cancellationToken);
      }
      finally
      {
        _gate.Release();
      }
    }

    private static int GetPosition(SourceText text, int line, int column)
    {
      var textLine = text.Lines[Math.Clamp(line - 1, 0, text.Lines.Count - 1)];
      return Math.Clamp(textLine.Start + column - 1, textLine.Start, textLine.End);
    }

    public Document UpdateCode(string code)
    {
      lock (_lock)
      {
        var newText = SourceText.From(code);

        var newSolution = _workspace.CurrentSolution.WithDocumentText(_document.Id, newText);

        _workspace.TryApplyChanges(newSolution);

        _document = _workspace.CurrentSolution.GetDocument(_document.Id);

        return _document;
      }
    }



    public Task<string> GetCode() => GetCode(CancellationToken.None);

    public Task<string> GetCode(CancellationToken cancellationToken) => RunExclusive(async ct =>
    {
      var result = await _document.GetTextAsync(ct);
      return result.ToString();
    }, cancellationToken);



    // Every public method has a version with and without a CancellationToken: the first for the IRosaCodeEngine
    // contract (also used by remote engines), the second for the editor, which cancels what it no longer needs.

    public Task<IReadOnlyList<ActionAction>> GetActions(string code, int line, int column, int endLine, int endColumn)
      => GetActions(code, line, column, endLine, endColumn, CancellationToken.None);

    public Task<IEnumerable<Completion>> GetCompletions(string code, int line, int column)
      => GetCompletions(code, line, column, CancellationToken.None);

    public Task<string> GetCompletionDescription(string code, int line, int column, int completionId)
      => GetCompletionDescription(code, line, column, completionId, CancellationToken.None);

    public Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces)
      => GetFormattedDocument(code, tabSize, insertSpaces, CancellationToken.None);

    public Task<IReadOnlyList<ActionEdit>> GetFormattedRange(string code, int startLine, int startColumn, int endLine, int endColumn, int tabSize, bool insertSpaces)
      => GetFormattedRange(code, startLine, startColumn, endLine, endColumn, tabSize, insertSpaces, CancellationToken.None);

    public Task<string> GetTooltip(string code, int line, int column)
      => GetTooltip(code, line, column, CancellationToken.None);

    public Task<IEnumerable<ActionDiagnostic>> GetDiagnostics(string code)
      => GetDiagnostics(code, CancellationToken.None);

    public Task<(IEnumerable<SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignatures(string code, int line, int column)
      => GetSignatures(code, line, column, CancellationToken.None);



    private readonly RoslynCodeActionProvider _codeActionProvider = new RoslynCodeActionProvider();

    // Code actions come from the code fix and refactoring providers that ship with Roslyn, as in Visual Studio.
    // The range is the selection (or the cursor): refactorings such as "Extract method" need a selection.
    public Task<IReadOnlyList<ActionAction>> GetActions(string code, int line, int column, int endLine, int endColumn, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var document = UpdateCode(code);
        var text = await document.GetTextAsync(ct);
        var start = GetPosition(text, line, column);
        var end = GetPosition(text, endLine, endColumn);
        var span = TextSpan.FromBounds(Math.Min(start, end), Math.Max(start, end));

        var results = await _codeActionProvider.GetActionsAsync(document, span, ct);

        var actions = new List<ActionAction>();
        foreach (var result in results
          .OrderByDescending(r => r.IsFix) // fixes before refactorings
          .ThenBy(r => r.Action.Title.StartsWith("using ") ? 0 : r.Action.Title.StartsWith("Generate") ? 2 : 1))
        {
          foreach (var (codeAction, title) in Flatten(result.Action, null))
          {
            ActionAction? action;
            try
            {
              action = await MapCodeActionToDto(codeAction, title, result, document, text, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
              // e.g. actions that need a dialog or a host service that is not available here
              System.Diagnostics.Trace.TraceWarning($"RosaCode: code action '{title}' skipped: {ex.Message}");
              continue;
            }

            if (action != null && !actions.Any(a => a.Title == action.Title))
              actions.Add(action);
          }
        }

        return (IReadOnlyList<ActionAction>)actions;
      }, cancellationToken);

    // Some actions only group others (e.g. "Introduce parameter for ..."); the editor gets the individual ones,
    // with the title of the group in front so that "and update call sites directly" keeps its context.
    private static IEnumerable<(CodeAction action, string title)> Flatten(CodeAction action, string? parentTitle)
    {
      var title = parentTitle == null || action.Title.StartsWith(parentTitle) ? action.Title : $"{parentTitle} {action.Title}";

      if (action.NestedActions.IsDefaultOrEmpty)
      {
        yield return (action, title);
        yield break;
      }

      foreach (var nested in action.NestedActions)
        foreach (var leaf in Flatten(nested, title))
          yield return leaf;
    }

    // A change of the text as an edit of the editor: lines and columns start at 1
    private static ActionRange CreateRange(SourceText text, TextSpan span)
    {
      var lines = text.Lines.GetLinePositionSpan(span);
      return new ActionRange
      {
        StartLineNumber = lines.Start.Line + 1,
        StartColumn = lines.Start.Character + 1,
        EndLineNumber = lines.End.Line + 1,
        EndColumn = lines.End.Character + 1,
      };
    }

    private static ActionEdit CreateEdit(SourceText text, TextChange change)
      => new ActionEdit { Range = CreateRange(text, change.Span), Text = change.NewText ?? string.Empty };

    private async Task<ActionAction?> MapCodeActionToDto(CodeAction codeAction, string title, RoslynCodeActionProvider.Result result, Document document, SourceText text, CancellationToken cancellationToken)
    {
      var operations = await codeAction.GetOperationsAsync(cancellationToken);
      var textEdits = new List<ActionEdit>();

      foreach (var operation in operations.OfType<ApplyChangesOperation>())
      {
        var changedDocument = operation.ChangedSolution.GetDocument(document.Id);
        if (changedDocument == null)
          continue;

        foreach (var change in await changedDocument.GetTextChangesAsync(document, cancellationToken))
        {
          textEdits.Add(CreateEdit(text, change));
        }
      }

      // e.g. actions that only start a rename or open a dialog
      if (textEdits.Count == 0)
        return null;

      return new ActionAction
      {
        Title = title,
        Kind = result.IsFix ? "quickfix" : "refactor",
        Diagnostics = result.Diagnostics.Select(Convert).ToList(),
        IsPreferred = false,
        Edit = new ActionEdits { Edits = textEdits }
      };
    }

  }
}
