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
    public int _EndLine { get; set; }
    public int _EndColumn { get; set; }

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
  public class RpcMessageGetCompletionDescriptionRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _Line { get; set; }
    public int _Column { get; set; }
    public int _CompletionId { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetCompletionDescriptionResponse: Lakerfield.Rpc.RpcMessage
  {
    public string Result { get; set; }
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
  public class RpcMessageGetFormattedRangeRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _StartLine { get; set; }
    public int _StartColumn { get; set; }
    public int _EndLine { get; set; }
    public int _EndColumn { get; set; }
    public int _TabSize { get; set; }
    public bool _InsertSpaces { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetFormattedRangeResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.ActionEdit> Result { get; set; }
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



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetDefinitionRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _Line { get; set; }
    public int _Column { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetDefinitionResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.SymbolLocation> Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetReferencesRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _Line { get; set; }
    public int _Column { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetReferencesResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.SymbolLocation> Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetRenameInfoRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _Line { get; set; }
    public int _Column { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetRenameInfoResponse: Lakerfield.Rpc.RpcMessage
  {
    public Lakerfield.RosaCode.RenameInfo Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetRenameEditsRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _Line { get; set; }
    public int _Column { get; set; }
    public string _NewName { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetRenameEditsResponse: Lakerfield.Rpc.RpcMessage
  {
    public Lakerfield.RosaCode.RenameResult Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetDocumentSymbolsRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetDocumentSymbolsResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.DocumentSymbolItem> Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetSemanticTokensRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetSemanticTokensResponse: Lakerfield.Rpc.RpcMessage
  {
    public Lakerfield.RosaCode.SemanticTokensResult Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetInlayHintsRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }
    public int _StartLine { get; set; }
    public int _StartColumn { get; set; }
    public int _EndLine { get; set; }
    public int _EndColumn { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetInlayHintsResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.InlayHintItem> Result { get; set; }
  }



  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetFoldingRangesRequest : Lakerfield.Rpc.RpcMessage
  {
    public string _Code { get; set; }

  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetFoldingRangesResponse: Lakerfield.Rpc.RpcMessage
  {
    public System.Collections.Generic.IReadOnlyList<Lakerfield.RosaCode.FoldingRangeItem> Result { get; set; }
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
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetCompletionDescriptionRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetCompletionDescriptionResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetFormattedDocumentRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetFormattedDocumentResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetFormattedRangeRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetFormattedRangeResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetTooltipRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetTooltipResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetDiagnosticsRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetDiagnosticsResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetSignaturesRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetSignaturesResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetDefinitionRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetDefinitionResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetReferencesRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetReferencesResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetRenameInfoRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetRenameInfoResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetRenameEditsRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetRenameEditsResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetDocumentSymbolsRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetDocumentSymbolsResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetSemanticTokensRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetSemanticTokensResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetInlayHintsRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetInlayHintsResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetFoldingRangesRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetFoldingRangesResponse>(AutoMap);

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
