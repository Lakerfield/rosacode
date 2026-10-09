using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using RpcDemo;

namespace RpcDemo.ServerApp
{
  public partial class MyRosaCodeEngineServer // global::RpcDemo.IRpcRosaCodeEngine
  {
    public MyRosaCodeEngineServer() : base ()
    {
      InitBsonClassMaps();
    }

    public override void InitBsonClassMaps()
    {
      RpcRosaCodeEngineBsonConfigurator.Configure();
    }

    //public override Lakerfield.Rpc.ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(Lakerfield.Rpc.LakerfieldRpcWebSocketServerConnection connection)
    //{
    //  return new Lakerfield.Rpc.LakerfieldRpcMessageRouter(connection);
    //}

    public partial class ClientConnectionMessageHandler : Lakerfield.Rpc.ILakerfieldRpcClientMessageHandler
    {
      public Lakerfield.Rpc.LakerfieldRpcWebSocketServerConnection Connection { get; }

      public ClientConnectionMessageHandler(Lakerfield.Rpc.LakerfieldRpcWebSocketServerConnection connection)
      {
        Connection = connection;
      }

      public Task<Lakerfield.Rpc.RpcMessage> HandleMessage(Lakerfield.Rpc.RpcMessage message)
      {
        if (message == null)
          throw new ArgumentNullException("message", "Cannot route null RpcMessage");

#if DEBUG
        System.Console.WriteLine($"new message {message.GetType().Name}");
#endif
        return message switch {
          RpcMessageGetCodeRequest request => _GetCode(request),
          RpcMessageGetActionsRequest request => _GetActions(request),
          RpcMessageGetCompletionsRequest request => _GetCompletions(request),
          RpcMessageGetCompletionDescriptionRequest request => _GetCompletionDescription(request),
          RpcMessageGetFormattedDocumentRequest request => _GetFormattedDocument(request),
          RpcMessageGetFormattedRangeRequest request => _GetFormattedRange(request),
          RpcMessageGetTooltipRequest request => _GetTooltip(request),
          RpcMessageGetDiagnosticsRequest request => _GetDiagnostics(request),
          RpcMessageGetSignaturesRequest request => _GetSignatures(request),
          RpcMessageGetDefinitionRequest request => _GetDefinition(request),
          RpcMessageGetReferencesRequest request => _GetReferences(request),
          RpcMessageGetRenameInfoRequest request => _GetRenameInfo(request),
          RpcMessageGetRenameEditsRequest request => _GetRenameEdits(request),
          RpcMessageGetDocumentSymbolsRequest request => _GetDocumentSymbols(request),
          RpcMessageGetSemanticTokensRequest request => _GetSemanticTokens(request),
          RpcMessageGetInlayHintsRequest request => _GetInlayHints(request),
          RpcMessageGetFoldingRangesRequest request => _GetFoldingRanges(request),

          _ => TaskNotImplementedMessage(message)
        };
      }

      private Task<Lakerfield.Rpc.RpcMessage> TaskNotImplementedMessage(Lakerfield.Rpc.RpcMessage message)
      {
        throw new NotImplementedException(string.Format("Message {0} not implemented", message.GetType().Name));
      }

      public Lakerfield.Rpc.NetworkObservable HandleObservable(Lakerfield.Rpc.RpcMessage message)
      {
        if (message == null)
          throw new ArgumentNullException("message", "Cannot route null RpcMessage");

#if DEBUG
        System.Console.WriteLine($"new message {message.GetType().Name}");
#endif
        return message switch {

          _ => ObservableNotImplementedMessage(message)
        };
      }

      private Lakerfield.Rpc.NetworkObservable ObservableNotImplementedMessage(Lakerfield.Rpc.RpcMessage message)
      {
        throw new NotImplementedException(string.Format("Message {0} not implemented", message.GetType().Name));
      }

      // GetCode already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetCode(RpcMessageGetCodeRequest request)
      {
        return new RpcMessageGetCodeResponse()
        {
          Result = await GetCode().ConfigureAwait(false)
        };
      }

      // GetActions already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetActions(RpcMessageGetActionsRequest request)
      {
        return new RpcMessageGetActionsResponse()
        {
          Result = await GetActions(request._Code, request._Line, request._Column, request._EndLine, request._EndColumn).ConfigureAwait(false)
        };
      }

      // GetCompletions already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetCompletions(RpcMessageGetCompletionsRequest request)
      {
        return new RpcMessageGetCompletionsResponse()
        {
          Result = await GetCompletions(request._Code, request._Line, request._Column).ConfigureAwait(false)
        };
      }

      // GetCompletionDescription already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetCompletionDescription(RpcMessageGetCompletionDescriptionRequest request)
      {
        return new RpcMessageGetCompletionDescriptionResponse()
        {
          Result = await GetCompletionDescription(request._Code, request._Line, request._Column, request._CompletionId).ConfigureAwait(false)
        };
      }

      // GetFormattedDocument already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetFormattedDocument(RpcMessageGetFormattedDocumentRequest request)
      {
        return new RpcMessageGetFormattedDocumentResponse()
        {
          Result = await GetFormattedDocument(request._Code, request._TabSize, request._InsertSpaces).ConfigureAwait(false)
        };
      }

      // GetFormattedRange already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetFormattedRange(RpcMessageGetFormattedRangeRequest request)
      {
        return new RpcMessageGetFormattedRangeResponse()
        {
          Result = await GetFormattedRange(request._Code, request._StartLine, request._StartColumn, request._EndLine, request._EndColumn, request._TabSize, request._InsertSpaces).ConfigureAwait(false)
        };
      }

      // GetTooltip already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetTooltip(RpcMessageGetTooltipRequest request)
      {
        return new RpcMessageGetTooltipResponse()
        {
          Result = await GetTooltip(request._Code, request._Line, request._Column).ConfigureAwait(false)
        };
      }

      // GetDiagnostics already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetDiagnostics(RpcMessageGetDiagnosticsRequest request)
      {
        return new RpcMessageGetDiagnosticsResponse()
        {
          Result = await GetDiagnostics(request._Code).ConfigureAwait(false)
        };
      }

      // GetSignatures already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetSignatures(RpcMessageGetSignaturesRequest request)
      {
        return new RpcMessageGetSignaturesResponse()
        {
          Result = await GetSignatures(request._Code, request._Line, request._Column).ConfigureAwait(false)
        };
      }

      // GetDefinition already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetDefinition(RpcMessageGetDefinitionRequest request)
      {
        return new RpcMessageGetDefinitionResponse()
        {
          Result = await GetDefinition(request._Code, request._Line, request._Column).ConfigureAwait(false)
        };
      }

      // GetReferences already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetReferences(RpcMessageGetReferencesRequest request)
      {
        return new RpcMessageGetReferencesResponse()
        {
          Result = await GetReferences(request._Code, request._Line, request._Column).ConfigureAwait(false)
        };
      }

      // GetRenameInfo already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetRenameInfo(RpcMessageGetRenameInfoRequest request)
      {
        return new RpcMessageGetRenameInfoResponse()
        {
          Result = await GetRenameInfo(request._Code, request._Line, request._Column).ConfigureAwait(false)
        };
      }

      // GetRenameEdits already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetRenameEdits(RpcMessageGetRenameEditsRequest request)
      {
        return new RpcMessageGetRenameEditsResponse()
        {
          Result = await GetRenameEdits(request._Code, request._Line, request._Column, request._NewName).ConfigureAwait(false)
        };
      }

      // GetDocumentSymbols already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetDocumentSymbols(RpcMessageGetDocumentSymbolsRequest request)
      {
        return new RpcMessageGetDocumentSymbolsResponse()
        {
          Result = await GetDocumentSymbols(request._Code).ConfigureAwait(false)
        };
      }

      // GetSemanticTokens already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetSemanticTokens(RpcMessageGetSemanticTokensRequest request)
      {
        return new RpcMessageGetSemanticTokensResponse()
        {
          Result = await GetSemanticTokens(request._Code).ConfigureAwait(false)
        };
      }

      // GetInlayHints already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetInlayHints(RpcMessageGetInlayHintsRequest request)
      {
        return new RpcMessageGetInlayHintsResponse()
        {
          Result = await GetInlayHints(request._Code, request._StartLine, request._StartColumn, request._EndLine, request._EndColumn).ConfigureAwait(false)
        };
      }

      // GetFoldingRanges already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetFoldingRanges(RpcMessageGetFoldingRangesRequest request)
      {
        return new RpcMessageGetFoldingRangesResponse()
        {
          Result = await GetFoldingRanges(request._Code).ConfigureAwait(false)
        };
      }



    }
  }
}
