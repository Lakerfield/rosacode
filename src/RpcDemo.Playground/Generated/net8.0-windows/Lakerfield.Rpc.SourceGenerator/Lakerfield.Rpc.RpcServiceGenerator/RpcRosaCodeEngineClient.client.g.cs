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
      var request = new RpcMessageGetCodeRequest() {  };
      var response = await Client.Execute<RpcMessageGetCodeResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionAction>> GetActions(string code, int line, int column, System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionDiagnostic> diagnostics)
    {
      var request = new RpcMessageGetActionsRequest() { _Code = code, _Line = line, _Column = column, _Diagnostics = diagnostics };
      var response = await Client.Execute<RpcMessageGetActionsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.Completion>> GetCompletions(string code, int line, int column)
    {
      var request = new RpcMessageGetCompletionsRequest() { _Code = code, _Line = line, _Column = column };
      var response = await Client.Execute<RpcMessageGetCompletionsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces)
    {
      var request = new RpcMessageGetFormattedDocumentRequest() { _Code = code, _TabSize = tabSize, _InsertSpaces = insertSpaces };
      var response = await Client.Execute<RpcMessageGetFormattedDocumentResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<string> GetTooltip(string code, int line, int column)
    {
      var request = new RpcMessageGetTooltipRequest() { _Code = code, _Line = line, _Column = column };
      var response = await Client.Execute<RpcMessageGetTooltipResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.ActionDiagnostic>> GetDiagnostics(string code)
    {
      var request = new RpcMessageGetDiagnosticsRequest() { _Code = code };
      var response = await Client.Execute<RpcMessageGetDiagnosticsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<(System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignatures(string code, int line, int column)
    {
      var request = new RpcMessageGetSignaturesRequest() { _Code = code, _Line = line, _Column = column };
      var response = await Client.Execute<RpcMessageGetSignaturesResponse>(request).ConfigureAwait(false);
      return response.Result;
    }


  }
}
