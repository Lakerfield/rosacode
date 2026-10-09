using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lakerfield.RosaCode
{
  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetCodeRequest : Lakerfield.Rpc.RpcMessage
  {

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetCodeResponse: Lakerfield.Rpc.RpcMessage
  {
    public string Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetActionsRequest : Lakerfield.Rpc.RpcMessage
  {
    public string Code { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionDiagnostic> Diagnostics { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetActionsResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionAction> Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetCompletionsRequest : Lakerfield.Rpc.RpcMessage
  {
    public string Code { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetCompletionsResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.Completion> Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetFormattedDocumentRequest : Lakerfield.Rpc.RpcMessage
  {
    public string Code { get; set; }
    public int TabSize { get; set; }
    public bool InsertSpaces { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetFormattedDocumentResponse: Lakerfield.Rpc.RpcMessage
  {
    public string Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetTooltipRequest : Lakerfield.Rpc.RpcMessage
  {
    public string Code { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetTooltipResponse: Lakerfield.Rpc.RpcMessage
  {
    public string Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetDiagnosticsRequest : Lakerfield.Rpc.RpcMessage
  {
    public string Code { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetDiagnosticsResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.ActionDiagnostic> Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetSignaturesRequest : Lakerfield.Rpc.RpcMessage
  {
    public string Code { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetSignaturesResponse: Lakerfield.Rpc.RpcMessage
  {
    public (System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.SignatureItem> signatures, int activeSignature, int activeParameter) Result { get; set; }
  }





  public static partial class MyRosaCodeEngineBsonConfigurator
  {

    private static bool _configured = false;
    public static void Configure()
    {
      if (_configured)
        return;

      _configured = true;

      PreConfigure();

      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetCodeRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetCodeResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetActionsRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetActionsResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetCompletionsRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetCompletionsResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetFormattedDocumentRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetFormattedDocumentResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetTooltipRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetTooltipResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetDiagnosticsRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetDiagnosticsResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetSignaturesRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetSignaturesResponse>(AutoMap);

      PostConfigure();
    }

    static partial void PreConfigure();
    static partial void PostConfigure();

    private static void AutoMap<T>(Lakerfield.Bson.Serialization.BsonClassMap<T> cm)
    {
      cm.AutoMap();
    }

    private static void AutoMapAndSetGenericDiscriminator(Lakerfield.Bson.Serialization.BsonClassMap cm)
    {
      cm.AutoMap();

      var cmType = cm.GetType();
      var cmGenericType = cmType.GenericTypeArguments.First();
      var discriminator = cmGenericType.Name;
      var cmGenericTypeType = cmGenericType.GenericTypeArguments.FirstOrDefault();
      if (cmGenericTypeType != null)
        discriminator += cmGenericTypeType.Name;
      cm.SetDiscriminator(discriminator);
    }

  }

}
