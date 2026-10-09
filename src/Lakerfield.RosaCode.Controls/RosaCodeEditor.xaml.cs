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
      PostWebMessage(-1, "setMode", Serialize((int)value));
    }

    private async void WebViewWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
      var message = Deserialize<WebMessage>(e.WebMessageAsJson);
      if (message == null) return;

      switch (message.Method)
      {
        case "action":
          var actionRequest = Deserialize<ActionRequest>(message.Json);
          try
          {
            var actions = await GetActionsAsync(Deserialize<string>(actionRequest.Code), actionRequest.Line, actionRequest.Column, actionRequest.Diagnostics);
            var actionResult = new ActionResponse() { Actions = actions };
            PostWebMessage(message.Id, message.Method, Serialize(actionResult));
          }
          catch (ArgumentOutOfRangeException)
          {
            //TODO: fix flow, do not use "old" diagnostics with new code
          }
          break;

        case "completion":
          var completionRequest = Deserialize<CompletionRequest>(message.Json);
          var completionResult = await GetCompletionsAsync(Deserialize<string>(completionRequest.Code), completionRequest.Line, completionRequest.Column);
          PostWebMessage(message.Id, message.Method, Serialize(completionResult));
          break;

        case "format":
          var formatRequest = Deserialize<FormatRequest>(message.Json);
          var format = await GetFormatAsync(Deserialize<string>(formatRequest.Code), formatRequest.TabSize, formatRequest.InsertSpaces);
          var formatResult = new FormatResponse() { Format = format };
          PostWebMessage(message.Id, message.Method, Serialize(formatResult));
          break;

        case "hover":
          var hoverRequest = Deserialize<HoverRequest>(message.Json);
          var hoverTooltip = await GetHoverAsync(Deserialize<string>(hoverRequest.Code), hoverRequest.Line, hoverRequest.Column);
          var hoverResult = new HoverResponse() { Tooltip = hoverTooltip };
          PostWebMessage(message.Id, message.Method, Serialize(hoverResult));
          break;

        //case "execute":
        //  var executionResult = await ExecuteCSharpAsync(message.Data.Code);
        //  SendMessageToWebView("executionResult", executionResult);
        //  break;

        case "diagnostics":
          var diagnosticRequest = Deserialize<DiagnosticsRequest>(message.Json);
          var diagnostics = await GetDiagnosticsAsync(Deserialize<string>(diagnosticRequest.Code));
          PostWebMessage(message.Id, message.Method, Serialize(diagnostics));
          break;

        case "signatures":
          var signatureRequest = Deserialize<CompletionRequest>(message.Json);
          var signatures = await GetSignaturesAsync(Deserialize<string>(signatureRequest.Code), signatureRequest.Line, signatureRequest.Column);
          PostWebMessage(message.Id, message.Method, Serialize(signatures));
          break;
      }
    }

    private async void WebViewNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
      _isEditorLoaded = true;
      SetCode(_pendingText);
    }

    public Task<string> GetCode()
    {
      return Engine.GetCode();
    }

    public void SetCode(string code)
    {
      if (_isEditorLoaded)
        PostWebMessage(-1, "setCode", Serialize(code));
      else
        _pendingText = code;
    }

    public async Task CleanupEditor()
    {
      if (webView?.CoreWebView2 != null)
      {
        webView.CoreWebView2.Stop();
        webView.CoreWebView2.Navigate("about:blank");
      }

      webView?.Dispose();
    }


    private async Task<IReadOnlyList<ActionAction>> GetActionsAsync(string code, int line, int column, IReadOnlyList<ActionDiagnostic> diagnostics)
    {
      var actions = await Engine.GetActions(code, line, column, diagnostics);

      return actions;
    }

    private async Task<Completion[]> GetCompletionsAsync(string code, int line, int column)
    {
      var completions = await Engine.GetCompletions(code, line, column);

      return completions.ToArray();
    }

    private async Task<string> GetFormatAsync(string code, int tabSize, bool insertSpaces)
    {
      var result = await Engine.GetFormattedDocument(code, tabSize, insertSpaces);

      return result;
    }

    private async Task<string> GetHoverAsync(string code, int line, int column)
    {
      var result = await Engine.GetTooltip(code, line, column);

      return result;
    }

    private async Task<DiagnosticsResponse> GetDiagnosticsAsync(string code)
    {
      HandleInternalTextChanged(code);

      var diagnostics = await Engine.GetDiagnostics(code);

      var result = new DiagnosticsResponse();
      foreach (var diagnostic in diagnostics)
      {
        result.Errors.Add(new DiagnosticItem()
        {
          Severity = diagnostic.Severity,
          Message = $"{diagnostic.Message.Replace("\"", "\\\"")}",
          StartLineNumber = diagnostic.StartLineNumber,
          StartColumn = diagnostic.StartColumn,
          EndLineNumber = diagnostic.EndLineNumber,
          EndColumn = diagnostic.EndColumn,
        });
      }
      return result;
    }

    private async Task<SignatureHelpResponse> GetSignaturesAsync(string code, int line, int column)
    {
      var (signatures, activeSignature, activeParameter) = await Engine.GetSignatures(code, line, column);

      return new SignatureHelpResponse()
      {
        Signatures = signatures.ToList(),
        ActiveSignature = activeSignature,
        ActiveParameter = activeParameter,
      };
    }





    private void PostWebMessage(string method, string data)
    {
      PostWebMessage(method, data);
    }

    private void PostWebMessage(int messageId, string method, string data)
    {
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

      using Stream stream = assembly.GetManifestResourceStream("Lakerfield.RosaCode.Resources.RosaCodeEditor.html");
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
