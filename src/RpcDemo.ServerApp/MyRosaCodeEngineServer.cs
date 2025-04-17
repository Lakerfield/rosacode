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

      public async Task<IReadOnlyList<ActionAction>> GetActions(string code, int line, int column, IReadOnlyList<ActionDiagnostic> diagnostics)
      {
        return await Engine.GetActions(code, line, column, diagnostics);
      }

      public async Task<IEnumerable<Completion>> GetCompletions(string code, int line, int column)
      {
        return await Engine.GetCompletions(code, line, column);
      }

      public async Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces)
      {
        return await Engine.GetFormattedDocument(code, tabSize, insertSpaces);
      }

      public async Task<string> GetTooltip(string code, int line, int column)
      {
        return await Engine.GetTooltip(code, line, column);
      }

      public async Task<IEnumerable<ActionDiagnostic>> GetDiagnostics(string code)
      {
        return await Engine.GetDiagnostics(code);
      }

      public async Task<(IEnumerable<SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignatures(string code, int line, int column)
      {
        return await Engine.GetSignatures(code, line, column);
      }
      //public async Task<Models.Company> CompanyFindById(System.Guid id)
      //{
      //  await Task.Delay(100);
      //  //this.Connection.TriggerClose();
      //  return new Models.Company()
      //  {
      //    Id = id.ToString(),
      //    Name = "The company",
      //    Remarks = "cool",
      //  };
      //}

      //public IObservable<RpcSample.Models.Company> GetObservable(System.Guid id)
      //{
      //  return Observable.Interval(TimeSpan.FromSeconds(1)).Select(i => new Models.Company()
      //  {
      //    Id = "TEST",
      //    Name = $"Company number {i}",
      //    Remarks = "x"
      //  }).Take(10);
      //}
    }
  }
}
