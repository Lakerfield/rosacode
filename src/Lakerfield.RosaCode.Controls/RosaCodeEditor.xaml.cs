using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lakerfield.RosaCode
{
  /// <summary>
  /// Interaction logic for RosaCodeEditor.xaml
  /// </summary>
  public partial class RosaCodeEditor : UserControl
  {
    private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private bool _isEditorLoaded = false;
    private readonly Dictionary<string, string> _queuedMessages = new();
    private readonly Dictionary<string, object?> _options = new();

    public IRosaCodeEngine Engine { get; private set; }

    private RosaCodeMode _mode = RosaCodeMode.Normal;
    public RosaCodeMode Mode
    {
      get { return _mode; }
      set
      {
        if (_mode == value)
          return;

        _mode = value;
        UpdateMode(value);
      }
    }


    public RosaCodeEditor()
    {
      InitializeComponent();

      LostFocus += OnLostFocus;
    }

    public async Task InitializeEditor(IRosaCodeEngine codeEditor, bool openDevTools = false)
    {
      if (Engine != null)
        throw new Exception("RosaCodeEditor.InitializeEditor can only be called one");

      Engine = codeEditor;
      
      await webView.EnsureCoreWebView2Async();

      string editorHtml = ReadEditorHtml();
      webView.NavigateToString(editorHtml);

      if (openDevTools)
        webView.CoreWebView2.OpenDevToolsWindow();
    }

    private void UpdateMode(RosaCodeMode value)
    {
      PostOrQueue("setMode", Serialize((int)value));
    }

    // Requests that are still running, by message id, so the editor can cancel them
    private readonly Dictionary<int, CancellationTokenSource> _runningRequests = new();

    private async void WebViewWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
      WebMessage? message = null;
      CancellationTokenSource? cts = null;
      try
      {
        message = Deserialize<WebMessage>(e.WebMessageAsJson);

        if (message.Id > 0) // 0 is a notification, nobody waits for an answer
        {
          cts = new CancellationTokenSource();
          _runningRequests[message.Id] = cts;
        }

        await HandleWebMessage(message, cts?.Token ?? CancellationToken.None);
      }
      catch (OperationCanceledException) when (cts?.IsCancellationRequested == true)
      {
        // the editor cancelled this request and no longer waits for the answer
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.TraceError($"RosaCode: '{message?.Method}' failed: {ex}");

        // Always answer, so the pending call in the editor is rejected instead of waiting forever.
        if (message != null && message.Id > 0)
        {
          try { PostWebMessage(message.Id, "error", Serialize(ex.Message)); }
          catch (Exception postEx) { System.Diagnostics.Trace.TraceError($"RosaCode: could not report error: {postEx.Message}"); }
        }
      }
      finally
      {
        if (message != null && cts != null)
        {
          _runningRequests.Remove(message.Id);
          cts.Dispose();
        }
      }
    }

    private async Task HandleWebMessage(WebMessage message, CancellationToken cancellationToken)
    {
      switch (message.Method)
      {
        case "ready":
          // Monaco is initialized in the page: send everything that was set before.
          _isEditorLoaded = true;
          foreach (var method in new[] { "setMode", "setTheme", "setOptions", "setDiagnosticsDelay" })
            if (_queuedMessages.Remove(method, out var queued))
              PostWebMessage(-1, method, queued);
          SetCode(_pendingText ?? string.Empty);
          if (_queuedMessages.Remove("setOriginalCode", out var queuedOriginal))
            PostWebMessage(-1, "setOriginalCode", queuedOriginal);
          break;

        case "textChanged":
          HandleInternalTextChanged(Deserialize<string>(Deserialize<DiagnosticsRequest>(message.Json).Code));
          break;

        case "cancel":
          if (_runningRequests.TryGetValue(Deserialize<CancelRequest>(message.Json).Id, out var running))
            running.Cancel();
          break;

        case "action":
          var actionRequest = Deserialize<ActionRequest>(message.Json);
          var actions = await GetActionsAsync(Deserialize<string>(actionRequest.Code), actionRequest.Line, actionRequest.Column, actionRequest.EndLine, actionRequest.EndColumn, cancellationToken);
          Reply(message, cancellationToken, new ActionResponse() { Actions = actions });
          break;

        case "completion":
          var completionRequest = Deserialize<CompletionRequest>(message.Json);
          var completionResult = await GetCompletionsAsync(Deserialize<string>(completionRequest.Code), completionRequest.Line, completionRequest.Column, cancellationToken);
          Reply(message, cancellationToken, completionResult);
          break;

        case "completionResolve":
          var resolveRequest = Deserialize<CompletionResolveRequest>(message.Json);
          var documentation = await GetCompletionDescriptionAsync(Deserialize<string>(resolveRequest.Code), resolveRequest.Line, resolveRequest.Column, resolveRequest.Id, cancellationToken);
          Reply(message, cancellationToken, new CompletionResolveResponse() { Documentation = documentation });
          break;

        case "format":
          var formatRequest = Deserialize<FormatRequest>(message.Json);
          var format = await GetFormatAsync(Deserialize<string>(formatRequest.Code), formatRequest.TabSize, formatRequest.InsertSpaces, cancellationToken);
          Reply(message, cancellationToken, new FormatResponse() { Format = format });
          break;

        case "formatRange":
          var formatRangeRequest = Deserialize<FormatRangeRequest>(message.Json);
          var formatEdits = await GetFormatRangeAsync(Deserialize<string>(formatRangeRequest.Code), formatRangeRequest, cancellationToken);
          Reply(message, cancellationToken, new FormatRangeResponse() { Edits = formatEdits });
          break;

        case "hover":
          var hoverRequest = Deserialize<HoverRequest>(message.Json);
          var hoverTooltip = await GetHoverAsync(Deserialize<string>(hoverRequest.Code), hoverRequest.Line, hoverRequest.Column, cancellationToken);
          Reply(message, cancellationToken, new HoverResponse() { Tooltip = hoverTooltip });
          break;

        case "diagnostics":
          var diagnosticRequest = Deserialize<DiagnosticsRequest>(message.Json);
          var diagnostics = await GetDiagnosticsAsync(Deserialize<string>(diagnosticRequest.Code), cancellationToken);
          Reply(message, cancellationToken, diagnostics);
          break;

        case "signatures":
          var signatureRequest = Deserialize<CompletionRequest>(message.Json);
          var signatures = await GetSignaturesAsync(Deserialize<string>(signatureRequest.Code), signatureRequest.Line, signatureRequest.Column, cancellationToken);
          Reply(message, cancellationToken, signatures);
          break;

        case "definition":
          var definitionRequest = Deserialize<CompletionRequest>(message.Json);
          var definitions = await CallEngine(cancellationToken,
            (engine, ct) => engine.GetDefinition(Deserialize<string>(definitionRequest.Code), definitionRequest.Line, definitionRequest.Column, ct),
            engine => engine.GetDefinition(Deserialize<string>(definitionRequest.Code), definitionRequest.Line, definitionRequest.Column));
          Reply(message, cancellationToken, new LocationsResponse() { Locations = definitions });
          break;

        case "references":
          var referencesRequest = Deserialize<CompletionRequest>(message.Json);
          var references = await CallEngine(cancellationToken,
            (engine, ct) => engine.GetReferences(Deserialize<string>(referencesRequest.Code), referencesRequest.Line, referencesRequest.Column, ct),
            engine => engine.GetReferences(Deserialize<string>(referencesRequest.Code), referencesRequest.Line, referencesRequest.Column));
          Reply(message, cancellationToken, new LocationsResponse() { Locations = references });
          break;

        case "renameInfo":
          var renameInfoRequest = Deserialize<CompletionRequest>(message.Json);
          var renameInfo = await CallEngine(cancellationToken,
            (engine, ct) => engine.GetRenameInfo(Deserialize<string>(renameInfoRequest.Code), renameInfoRequest.Line, renameInfoRequest.Column, ct),
            engine => engine.GetRenameInfo(Deserialize<string>(renameInfoRequest.Code), renameInfoRequest.Line, renameInfoRequest.Column));
          Reply(message, cancellationToken, renameInfo);
          break;

        case "rename":
          var renameRequest = Deserialize<RenameRequest>(message.Json);
          var renameResult = await CallEngine(cancellationToken,
            (engine, ct) => engine.GetRenameEdits(Deserialize<string>(renameRequest.Code), renameRequest.Line, renameRequest.Column, renameRequest.NewName, ct),
            engine => engine.GetRenameEdits(Deserialize<string>(renameRequest.Code), renameRequest.Line, renameRequest.Column, renameRequest.NewName));
          Reply(message, cancellationToken, renameResult);
          break;

        case "symbols":
          var symbolsRequest = Deserialize<DiagnosticsRequest>(message.Json);
          var symbols = await CallEngine(cancellationToken,
            (engine, ct) => engine.GetDocumentSymbols(Deserialize<string>(symbolsRequest.Code), ct),
            engine => engine.GetDocumentSymbols(Deserialize<string>(symbolsRequest.Code)));
          Reply(message, cancellationToken, new SymbolsResponse() { Symbols = symbols });
          break;

        case "semanticTokens":
          var tokensRequest = Deserialize<DiagnosticsRequest>(message.Json);
          var tokens = await CallEngine(cancellationToken,
            (engine, ct) => engine.GetSemanticTokens(Deserialize<string>(tokensRequest.Code), ct),
            engine => engine.GetSemanticTokens(Deserialize<string>(tokensRequest.Code)));
          Reply(message, cancellationToken, tokens);
          break;

        case "inlayHints":
          var hintsRequest = Deserialize<RangeRequest>(message.Json);
          var hints = await CallEngine(cancellationToken,
            (engine, ct) => engine.GetInlayHints(Deserialize<string>(hintsRequest.Code), hintsRequest.StartLine, hintsRequest.StartColumn, hintsRequest.EndLine, hintsRequest.EndColumn, ct),
            engine => engine.GetInlayHints(Deserialize<string>(hintsRequest.Code), hintsRequest.StartLine, hintsRequest.StartColumn, hintsRequest.EndLine, hintsRequest.EndColumn));
          Reply(message, cancellationToken, new InlayHintsResponse() { Hints = hints });
          break;

        case "folding":
          var foldingRequest = Deserialize<DiagnosticsRequest>(message.Json);
          var folding = await CallEngine(cancellationToken,
            (engine, ct) => engine.GetFoldingRanges(Deserialize<string>(foldingRequest.Code), ct),
            engine => engine.GetFoldingRanges(Deserialize<string>(foldingRequest.Code)));
          Reply(message, cancellationToken, new FoldingResponse() { Ranges = folding });
          break;
      }
    }

    private void Reply(WebMessage request, CancellationToken cancellationToken, object response)
    {
      cancellationToken.ThrowIfCancellationRequested();
      PostWebMessage(request.Id, request.Method, Serialize(response));
    }

    /// <summary>The text in the editor. Not asked from the engine, which only knows the code of its last request.</summary>
    public Task<string> GetCode()
    {
      return Task.FromResult(_pendingText ?? Text ?? string.Empty);
    }

    public void SetCode(string code)
    {
      if (_isEditorLoaded)
        PostWebMessage(-1, "setCode", Serialize(code));
      else
        _pendingText = code;
    }

    /// <summary>Sets the original (left) side of the diff view. By default it is the last code set with <see cref="SetCode"/>.</summary>
    public void SetOriginalCode(string code)
    {
      PostOrQueue("setOriginalCode", Serialize(code));
    }

    /// <summary>Sets the Monaco theme: "vs", "vs-dark" (default) or "hc-black".</summary>
    public void SetTheme(string theme)
    {
      PostOrQueue("setTheme", Serialize(theme));
    }

    /// <summary>How long the text must stay unchanged before it is checked for errors. The default is one second.</summary>
    public void SetDiagnosticsDelay(TimeSpan delay)
    {
      PostOrQueue("setDiagnosticsDelay", Serialize((int)Math.Max(0, delay.TotalMilliseconds)));
    }

    /// <summary>Sets Monaco editor options (https://microsoft.github.io/monaco-editor/docs.html), e.g. { ["readOnly"] = true, ["fontSize"] = 14 }.</summary>
    public void SetOptions(IReadOnlyDictionary<string, object?> options)
    {
      foreach (var option in options)
        _options[option.Key] = option.Value;

      // before the editor is ready everything is sent at once, afterwards only the changes
      PostOrQueue("setOptions", Serialize(_isEditorLoaded ? options : _options));
    }

    private void PostOrQueue(string method, string json)
    {
      if (_isEditorLoaded)
        PostWebMessage(-1, method, json);
      else
        _queuedMessages[method] = json;
    }

    public Task CleanupEditor()
    {
      foreach (var running in _runningRequests.Values.ToList())
        running.Cancel();

      _queuedMessages.Clear();
      _isEditorLoaded = false;

      if (webView?.CoreWebView2 != null)
      {
        webView.CoreWebView2.Stop();
        webView.CoreWebView2.Navigate("about:blank");
      }

      webView?.Dispose();

      return Task.CompletedTask;
    }


    // Calls the engine with the token when it supports cancellation, and drops the answer when the editor cancelled in the meantime
    private async Task<T> CallEngine<T>(
      CancellationToken cancellationToken,
      Func<ICancellableRosaCodeEngine, CancellationToken, Task<T>> cancellable,
      Func<IRosaCodeEngine, Task<T>> plain)
    {
      var result = Engine is ICancellableRosaCodeEngine engine
        ? await cancellable(engine, cancellationToken)
        : await plain(Engine);

      cancellationToken.ThrowIfCancellationRequested();
      return result;
    }

    // The engine gets the token when it supports cancellation (the Roslyn engine does, a remote engine cannot).
    // Either way the answer is dropped when the editor cancelled in the meantime.

    private async Task<IReadOnlyList<ActionAction>> GetActionsAsync(string code, int line, int column, int endLine, int endColumn, CancellationToken cancellationToken)
    {
      var actions = Engine is ICancellableRosaCodeEngine cancellable
        ? await cancellable.GetActions(code, line, column, endLine, endColumn, cancellationToken)
        : await Engine.GetActions(code, line, column, endLine, endColumn);

      cancellationToken.ThrowIfCancellationRequested();
      return actions;
    }

    private async Task<Completion[]> GetCompletionsAsync(string code, int line, int column, CancellationToken cancellationToken)
    {
      var completions = Engine is ICancellableRosaCodeEngine cancellable
        ? await cancellable.GetCompletions(code, line, column, cancellationToken)
        : await Engine.GetCompletions(code, line, column);

      cancellationToken.ThrowIfCancellationRequested();
      return completions.ToArray();
    }

    private async Task<string> GetCompletionDescriptionAsync(string code, int line, int column, int completionId, CancellationToken cancellationToken)
    {
      var result = Engine is ICancellableRosaCodeEngine cancellable
        ? await cancellable.GetCompletionDescription(code, line, column, completionId, cancellationToken)
        : await Engine.GetCompletionDescription(code, line, column, completionId);

      cancellationToken.ThrowIfCancellationRequested();
      return result ?? string.Empty;
    }

    private async Task<string> GetFormatAsync(string code, int tabSize, bool insertSpaces, CancellationToken cancellationToken)
    {
      var result = Engine is ICancellableRosaCodeEngine cancellable
        ? await cancellable.GetFormattedDocument(code, tabSize, insertSpaces, cancellationToken)
        : await Engine.GetFormattedDocument(code, tabSize, insertSpaces);

      cancellationToken.ThrowIfCancellationRequested();
      return result;
    }

    private async Task<IReadOnlyList<ActionEdit>> GetFormatRangeAsync(string code, FormatRangeRequest request, CancellationToken cancellationToken)
    {
      var result = Engine is ICancellableRosaCodeEngine cancellable
        ? await cancellable.GetFormattedRange(code, request.StartLine, request.StartColumn, request.EndLine, request.EndColumn, request.TabSize, request.InsertSpaces, cancellationToken)
        : await Engine.GetFormattedRange(code, request.StartLine, request.StartColumn, request.EndLine, request.EndColumn, request.TabSize, request.InsertSpaces);

      cancellationToken.ThrowIfCancellationRequested();
      return result;
    }

    private async Task<string> GetHoverAsync(string code, int line, int column, CancellationToken cancellationToken)
    {
      var result = Engine is ICancellableRosaCodeEngine cancellable
        ? await cancellable.GetTooltip(code, line, column, cancellationToken)
        : await Engine.GetTooltip(code, line, column);

      cancellationToken.ThrowIfCancellationRequested();
      return result ?? string.Empty;
    }

    private async Task<DiagnosticsResponse> GetDiagnosticsAsync(string code, CancellationToken cancellationToken)
    {
      HandleInternalTextChanged(code);

      var diagnostics = Engine is ICancellableRosaCodeEngine cancellable
        ? await cancellable.GetDiagnostics(code, cancellationToken)
        : await Engine.GetDiagnostics(code);

      cancellationToken.ThrowIfCancellationRequested();

      var result = new DiagnosticsResponse();
      foreach (var diagnostic in diagnostics)
      {
        result.Errors.Add(new DiagnosticItem()
        {
          Severity = diagnostic.Severity,
          Message = diagnostic.Message,
          StartLineNumber = diagnostic.StartLineNumber,
          StartColumn = diagnostic.StartColumn,
          EndLineNumber = diagnostic.EndLineNumber,
          EndColumn = diagnostic.EndColumn,
          Id = diagnostic.Id,
          HelpLink = diagnostic.HelpLink,
          Tags = diagnostic.Tags,
        });
      }
      return result;
    }

    private async Task<SignatureHelpResponse> GetSignaturesAsync(string code, int line, int column, CancellationToken cancellationToken)
    {
      var (signatures, activeSignature, activeParameter) = Engine is ICancellableRosaCodeEngine cancellable
        ? await cancellable.GetSignatures(code, line, column, cancellationToken)
        : await Engine.GetSignatures(code, line, column);

      cancellationToken.ThrowIfCancellationRequested();

      return new SignatureHelpResponse()
      {
        Signatures = signatures.ToList(),
        ActiveSignature = activeSignature,
        ActiveParameter = activeParameter,
      };
    }



    private void PostWebMessage(int messageId, string method, string data)
    {
      if (webView?.CoreWebView2 == null)
        return;

      var json = Serialize(new WebMessage
      {
        Id = messageId,
        Method = method,
        Json = data
      });

      webView.CoreWebView2.PostWebMessageAsJson(json);
    }

    private string Serialize(object data)
    {
      return JsonSerializer.Serialize(data, _serializerOptions);
    }

    private T Deserialize<T>(string json)
    {
      var result = JsonSerializer.Deserialize<T>(json, _serializerOptions);
      if (result == null)
        throw new Exception("Deserialize failed");
      return result;
    }


    private string ReadEditorHtml()
    {
      var assembly = Assembly.GetExecutingAssembly();

      const string resourceName = "Lakerfield.RosaCode.Resources.RosaCodeEditor.html";
      using Stream stream = assembly.GetManifestResourceStream(resourceName)
        ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found in {assembly.GetName().Name}");
      using StreamReader reader = new StreamReader(stream);

      return reader.ReadToEnd();
    }




    private bool _suppressTextChangedCallback;
    private string _pendingText;

    public string Text
    {
      get => (string)GetValue(TextProperty);
      set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(RosaCodeEditor),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged));

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var control = (RosaCodeEditor)d;

      control.OnTextChanged((string)e.OldValue, (string)e.NewValue);
    }

    protected virtual void OnTextChanged(string oldValue, string newValue)
    {
      if (_pendingText == newValue)
        return;

      _pendingText = newValue;

      if (_suppressTextChangedCallback)
        return;

      SetCode(newValue);
    }

    public void HandleInternalTextChanged(string newText)
    {
      var bindingExpr = BindingOperations.GetBindingExpression(this, TextProperty);
      var trigger = bindingExpr?.ParentBinding?.UpdateSourceTrigger ?? UpdateSourceTrigger.Default;

      if (trigger == UpdateSourceTrigger.PropertyChanged)
      {
        _suppressTextChangedCallback = true;
        try
        {
          SetValue(TextProperty, newText); // Still propagates to binding
        }
        finally
        {
          _suppressTextChangedCallback = false;
        }
      }
      else if (trigger == UpdateSourceTrigger.LostFocus || trigger == UpdateSourceTrigger.Default)
      {
        _pendingText = newText;
      }
      else if (trigger == UpdateSourceTrigger.Explicit)
      {
        _pendingText = newText;
      }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
      var bindingExpr = BindingOperations.GetBindingExpression(this, TextProperty);
      var trigger = bindingExpr?.ParentBinding?.UpdateSourceTrigger ?? UpdateSourceTrigger.Default;

      if ((trigger == UpdateSourceTrigger.LostFocus || trigger == UpdateSourceTrigger.Default)
          && _pendingText != Text)
      {
        SetCurrentValue(TextProperty, _pendingText);
      }
    }

    public void CommitTextToBinding()
    {
      var bindingExpr = BindingOperations.GetBindingExpression(this, TextProperty);
      if (bindingExpr != null)
      {
        SetCurrentValue(TextProperty, _pendingText);
        bindingExpr.UpdateSource();
      }
    }

  }

}
