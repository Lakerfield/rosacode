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

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionAction>> GetActions(string code, int line, int column, int endLine, int endColumn)
    {
      var request = new RpcMessageGetActionsRequest() { _Code = code, _Line = line, _Column = column, _EndLine = endLine, _EndColumn = endColumn };
      var response = await Client.Execute<RpcMessageGetActionsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.Completion>> GetCompletions(string code, int line, int column)
    {
      var request = new RpcMessageGetCompletionsRequest() { _Code = code, _Line = line, _Column = column };
      var response = await Client.Execute<RpcMessageGetCompletionsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<string> GetCompletionDescription(string code, int line, int column, int completionId)
    {
      var request = new RpcMessageGetCompletionDescriptionRequest() { _Code = code, _Line = line, _Column = column, _CompletionId = completionId };
      var response = await Client.Execute<RpcMessageGetCompletionDescriptionResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces)
    {
      var request = new RpcMessageGetFormattedDocumentRequest() { _Code = code, _TabSize = tabSize, _InsertSpaces = insertSpaces };
      var response = await Client.Execute<RpcMessageGetFormattedDocumentResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionEdit>> GetFormattedRange(string code, int startLine, int startColumn, int endLine, int endColumn, int tabSize, bool insertSpaces)
    {
      var request = new RpcMessageGetFormattedRangeRequest() { _Code = code, _StartLine = startLine, _StartColumn = startColumn, _EndLine = endLine, _EndColumn = endColumn, _TabSize = tabSize, _InsertSpaces = insertSpaces };
      var response = await Client.Execute<RpcMessageGetFormattedRangeResponse>(request).ConfigureAwait(false);
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

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.SymbolLocation>> GetDefinition(string code, int line, int column)
    {
      var request = new RpcMessageGetDefinitionRequest() { _Code = code, _Line = line, _Column = column };
      var response = await Client.Execute<RpcMessageGetDefinitionResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.SymbolLocation>> GetReferences(string code, int line, int column)
    {
      var request = new RpcMessageGetReferencesRequest() { _Code = code, _Line = line, _Column = column };
      var response = await Client.Execute<RpcMessageGetReferencesResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<Lakerfield.RosaCode.RenameInfo> GetRenameInfo(string code, int line, int column)
    {
      var request = new RpcMessageGetRenameInfoRequest() { _Code = code, _Line = line, _Column = column };
      var response = await Client.Execute<RpcMessageGetRenameInfoResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<Lakerfield.RosaCode.RenameResult> GetRenameEdits(string code, int line, int column, string newName)
    {
      var request = new RpcMessageGetRenameEditsRequest() { _Code = code, _Line = line, _Column = column, _NewName = newName };
      var response = await Client.Execute<RpcMessageGetRenameEditsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.DocumentSymbolItem>> GetDocumentSymbols(string code)
    {
      var request = new RpcMessageGetDocumentSymbolsRequest() { _Code = code };
      var response = await Client.Execute<RpcMessageGetDocumentSymbolsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<Lakerfield.RosaCode.SemanticTokensResult> GetSemanticTokens(string code)
    {
      var request = new RpcMessageGetSemanticTokensRequest() { _Code = code };
      var response = await Client.Execute<RpcMessageGetSemanticTokensResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.InlayHintItem>> GetInlayHints(string code, int startLine, int startColumn, int endLine, int endColumn)
    {
      var request = new RpcMessageGetInlayHintsRequest() { _Code = code, _StartLine = startLine, _StartColumn = startColumn, _EndLine = endLine, _EndColumn = endColumn };
      var response = await Client.Execute<RpcMessageGetInlayHintsResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.FoldingRangeItem>> GetFoldingRanges(string code)
    {
      var request = new RpcMessageGetFoldingRangesRequest() { _Code = code };
      var response = await Client.Execute<RpcMessageGetFoldingRangesResponse>(request).ConfigureAwait(false);
      return response.Result;
    }


  }
}
