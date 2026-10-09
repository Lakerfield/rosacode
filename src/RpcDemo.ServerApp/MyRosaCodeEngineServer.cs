using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lakerfield.RosaCode;
using Lakerfield.Rpc;

namespace RpcDemo.ServerApp
{
  [RpcServer]
  public partial class MyRosaCodeEngineServer : Lakerfield.Rpc.LakerfieldRpcWebSocketServer<IRpcRosaCodeEngine>
  {
    public override ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(LakerfieldRpcWebSocketServerConnection connection)
    {
      return new ClientConnectionMessageHandler(connection as LakerfieldRpcWebSocketServerConnection<IRpcRosaCodeEngine>);
    }


    public partial class ClientConnectionMessageHandler
    {
      private RosaCodeRoslynEngine _engine;

      public RosaCodeRoslynEngine Engine
      {
        get
        {
          return _engine ??= new RosaCodeRoslynEngine();
        }
      }

      public async Task<string> GetCode()
      {
        return await Engine.GetCode();
      }

      public async Task<IReadOnlyList<ActionAction>> GetActions(string code, int line, int column, int endLine, int endColumn)
      {
        return await Engine.GetActions(code, line, column, endLine, endColumn);
      }

      public async Task<IEnumerable<Completion>> GetCompletions(string code, int line, int column)
      {
        return await Engine.GetCompletions(code, line, column);
      }

      public async Task<string> GetCompletionDescription(string code, int line, int column, int completionId)
      {
        return await Engine.GetCompletionDescription(code, line, column, completionId);
      }

      public async Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces)
      {
        return await Engine.GetFormattedDocument(code, tabSize, insertSpaces);
      }

      public async Task<IReadOnlyList<ActionEdit>> GetFormattedRange(string code, int startLine, int startColumn, int endLine, int endColumn, int tabSize, bool insertSpaces)
      {
        return await Engine.GetFormattedRange(code, startLine, startColumn, endLine, endColumn, tabSize, insertSpaces);
      }

      public async Task<string> GetTooltip(string code, int line, int column)
      {
        return await Engine.GetTooltip(code, line, column);
      }

      public async Task<IEnumerable<ActionDiagnostic>> GetDiagnostics(string code)
      {
        return await Engine.GetDiagnostics(code);
      }

      public async Task<IReadOnlyList<SymbolLocation>> GetDefinition(string code, int line, int column)
      {
        return await Engine.GetDefinition(code, line, column);
      }

      public async Task<IReadOnlyList<SymbolLocation>> GetReferences(string code, int line, int column)
      {
        return await Engine.GetReferences(code, line, column);
      }

      public async Task<RenameInfo> GetRenameInfo(string code, int line, int column)
      {
        return await Engine.GetRenameInfo(code, line, column);
      }

      public async Task<RenameResult> GetRenameEdits(string code, int line, int column, string newName)
      {
        return await Engine.GetRenameEdits(code, line, column, newName);
      }

      public async Task<IReadOnlyList<DocumentSymbolItem>> GetDocumentSymbols(string code)
      {
        return await Engine.GetDocumentSymbols(code);
      }

      public async Task<SemanticTokensResult> GetSemanticTokens(string code)
      {
        return await Engine.GetSemanticTokens(code);
      }

      public async Task<IReadOnlyList<InlayHintItem>> GetInlayHints(string code, int startLine, int startColumn, int endLine, int endColumn)
      {
        return await Engine.GetInlayHints(code, startLine, startColumn, endLine, endColumn);
      }

      public async Task<IReadOnlyList<FoldingRangeItem>> GetFoldingRanges(string code)
      {
        return await Engine.GetFoldingRanges(code);
      }

      public async Task<(IEnumerable<SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignatures(string code, int line, int column)
      {
        return await Engine.GetSignatures(code, line, column);
      }

    }
  }
}
