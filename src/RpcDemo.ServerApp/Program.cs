using Lakerfield.RosaCode;
using Lakerfield.Rpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace RpcDemo.ServerApp
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddSingleton<MyRosaCodeEngineServer>();
      builder.Services.AddRpcWebSocketServer<IRpcRosaCodeEngine, MyRosaCodeEngineServer>();

      var app = builder.Build();

      app.MapGet("/", () => "Hello World!");


      var webSocketOptions = new WebSocketOptions
      {
        KeepAliveInterval = TimeSpan.FromSeconds(120),
        AllowedOrigins = { "*" } // Pas dit aan voor productie!
      };

      app.UseWebSockets(webSocketOptions);

      app.UseRpcWebSocketServer<IRpcRosaCodeEngine>("/ws");

      await app.RunAsync();
    }
  }
}
