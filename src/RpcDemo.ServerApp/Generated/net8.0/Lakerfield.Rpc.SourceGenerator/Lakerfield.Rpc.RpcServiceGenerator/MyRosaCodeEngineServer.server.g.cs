using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using RpcDemo;

namespace RpcDemo.ServerApp
{
  public partial class MyRosaCodeEngineServer // RpcDemo.IRpcRosaCodeEngine
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
          GetCodeRequest request => _GetCode(request),
          GetActionsRequest request => _GetActions(request),
          GetCompletionsRequest request => _GetCompletions(request),
          GetFormattedDocumentRequest request => _GetFormattedDocument(request),
          GetTooltipRequest request => _GetTooltip(request),
          GetDiagnosticsRequest request => _GetDiagnostics(request),
          GetSignaturesRequest request => _GetSignatures(request),

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
      public async Task<Lakerfield.Rpc.RpcMessage> _GetCode(GetCodeRequest request)
      {
        return new GetCodeResponse()
        {
          Result = await GetCode().ConfigureAwait(false)
        };
      }

      // GetActions already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetActions(GetActionsRequest request)
      {
        return new GetActionsResponse()
        {
          Result = await GetActions(request.Code, request.Line, request.Column, request.Diagnostics).ConfigureAwait(false)
        };
      }

      // GetCompletions already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetCompletions(GetCompletionsRequest request)
      {
        return new GetCompletionsResponse()
        {
          Result = await GetCompletions(request.Code, request.Line, request.Column).ConfigureAwait(false)
        };
      }

      // GetFormattedDocument already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetFormattedDocument(GetFormattedDocumentRequest request)
      {
        return new GetFormattedDocumentResponse()
        {
          Result = await GetFormattedDocument(request.Code, request.TabSize, request.InsertSpaces).ConfigureAwait(false)
        };
      }

      // GetTooltip already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetTooltip(GetTooltipRequest request)
      {
        return new GetTooltipResponse()
        {
          Result = await GetTooltip(request.Code, request.Line, request.Column).ConfigureAwait(false)
        };
      }

      // GetDiagnostics already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetDiagnostics(GetDiagnosticsRequest request)
      {
        return new GetDiagnosticsResponse()
        {
          Result = await GetDiagnostics(request.Code).ConfigureAwait(false)
        };
      }

      // GetSignatures already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetSignatures(GetSignaturesRequest request)
      {
        return new GetSignaturesResponse()
        {
          Result = await GetSignatures(request.Code, request.Line, request.Column).ConfigureAwait(false)
        };
      }



    }
  }
}
