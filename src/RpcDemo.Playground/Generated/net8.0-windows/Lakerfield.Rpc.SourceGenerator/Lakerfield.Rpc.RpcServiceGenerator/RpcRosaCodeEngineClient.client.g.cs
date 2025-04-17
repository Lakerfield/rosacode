using System;
using RpcDemo;

namespace RpcDemo
{
  public partial class RpcRosaCodeEngineClient
  {
    public Lakerfield.Rpc.INetworkClient Client { get; }
    public RpcRosaCodeEngineClient(Lakerfield.Rpc.INetworkClient client)
    {
      RpcRosaCodeEngineBsonConfigurator.Configure();
      Client = client;
    }

    public async System.Threading.Tasks.Task<string> GetCode()
    {
      var request = new GetCodeRequest() {  };
      var response = await Client.Execute<GetCodeResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionAction>> GetActions(string code, int line, int column, System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionDiagnostic> diagnostics)
    {
      var request = new GetActionsRequest() { Code = code, Line = line, Column = column, Diagnostics = diagnostics };
      var response = await Client.Execute<GetActionsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.Completion>> GetCompletions(string code, int line, int column)
    {
      var request = new GetCompletionsRequest() { Code = code, Line = line, Column = column };
      var response = await Client.Execute<GetCompletionsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces)
    {
      var request = new GetFormattedDocumentRequest() { Code = code, TabSize = tabSize, InsertSpaces = insertSpaces };
      var response = await Client.Execute<GetFormattedDocumentResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<string> GetTooltip(string code, int line, int column)
    {
      var request = new GetTooltipRequest() { Code = code, Line = line, Column = column };
      var response = await Client.Execute<GetTooltipResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.ActionDiagnostic>> GetDiagnostics(string code)
    {
      var request = new GetDiagnosticsRequest() { Code = code };
      var response = await Client.Execute<GetDiagnosticsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<(System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignatures(string code, int line, int column)
    {
      var request = new GetSignaturesRequest() { Code = code, Line = line, Column = column };
      var response = await Client.Execute<GetSignaturesResponse>(request).ConfigureAwait(false);
      return response.Result;
    }


  }
}
