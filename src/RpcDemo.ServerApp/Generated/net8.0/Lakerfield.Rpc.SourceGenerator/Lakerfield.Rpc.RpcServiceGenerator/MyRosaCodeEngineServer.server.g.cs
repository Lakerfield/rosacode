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
          RpcMessageGetFormattedDocumentRequest request => _GetFormattedDocument(request),
          RpcMessageGetTooltipRequest request => _GetTooltip(request),
          RpcMessageGetDiagnosticsRequest request => _GetDiagnostics(request),
          RpcMessageGetSignaturesRequest request => _GetSignatures(request),

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
          Result = await GetActions(request._Code, request._Line, request._Column, request._Diagnostics).ConfigureAwait(false)
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

      // GetFormattedDocument already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _GetFormattedDocument(RpcMessageGetFormattedDocumentRequest request)
      {
        return new RpcMessageGetFormattedDocumentResponse()
        {
          Result = await GetFormattedDocument(request._Code, request._TabSize, request._InsertSpaces).ConfigureAwait(false)
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



    }
  }
}
