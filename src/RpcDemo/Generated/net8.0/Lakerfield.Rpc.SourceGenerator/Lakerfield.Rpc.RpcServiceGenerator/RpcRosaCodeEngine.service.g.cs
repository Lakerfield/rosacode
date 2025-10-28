using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RpcDemo
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetCodeRequest : Lakerfield.Rpc.RpcMessage
  {

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetCodeResponse: Lakerfield.Rpc.RpcMessage
  {
    public string Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetActionsRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _Line { get; set; }
    public int _Column { get; set; }
    public System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionDiagnostic> _Diagnostics { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetActionsResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionAction> Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetCompletionsRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _Line { get; set; }
    public int _Column { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetCompletionsResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.Completion> Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetFormattedDocumentRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _TabSize { get; set; }
    public bool _InsertSpaces { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetFormattedDocumentResponse: Lakerfield.Rpc.RpcMessage
  {
    public string Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetTooltipRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _Line { get; set; }
    public int _Column { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetTooltipResponse: Lakerfield.Rpc.RpcMessage
  {
    public string Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetDiagnosticsRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetDiagnosticsResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.ActionDiagnostic> Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetSignaturesRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _Line { get; set; }
    public int _Column { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetSignaturesResponse: Lakerfield.Rpc.RpcMessage
  {
    public (System.Collections.Generic.IEnumerable<Lakerfield.RosaCode.SignatureItem> signatures, int activeSignature, int activeParameter) Result { get; set; }
  }





  public static partial class RpcRosaCodeEngineBsonConfigurator
  {

    private static bool _configured = false;
    public static void Configure()
    {
      if (_configured)
        return;

      _configured = true;

      PreConfigure();

      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetCodeRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetCodeResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetActionsRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetActionsResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetCompletionsRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetCompletionsResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetFormattedDocumentRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetFormattedDocumentResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetTooltipRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetTooltipResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetDiagnosticsRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetDiagnosticsResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetSignaturesRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetSignaturesResponse>(AutoMap);

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
