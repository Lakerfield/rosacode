using System.Reflection;
using System.Text.Json;
using Lakerfield.Bson.Serialization;
using Lakerfield.Bson.Serialization.Serializers;
using Lakerfield.RosaCode;
using Lakerfield.Rpc;

namespace RpcDemo;

[RpcService]
public interface IRpcRosaCodeEngine : IRosaCodeEngine
{

}

public static partial class RpcRosaCodeEngineBsonConfigurator
{
  private static bool IsAllowedType(Type type)
  {
    return type.IsConstructedGenericType ?
      type.GetGenericArguments().All(IsAllowedType) :
      type.FullName.StartsWith("RpcSample");
  }

  static partial void PreConfigure()
  {
    //var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);
    var objectSerializer = new ObjectSerializer(type => ObjectSerializer.DefaultAllowedTypes(type) || IsAllowedType(type));
    BsonSerializer.RegisterSerializer(objectSerializer);

    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessage>();
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcExceptionMessage>();
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcObservableMessage>();

    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<ActionAction> (AutoMap);
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<ActionDiagnostic> (AutoMap);
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<ActionEdits> (AutoMap);
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<ActionEdit> (AutoMap);
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<ActionRange> (AutoMap);

    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<Completion> (AutoMap);

    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<DiagnosticItem> (AutoMap);

    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<SignatureItem> (AutoMap);
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<MarkdownValue> (AutoMap);
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<SignatureItemParameter> (AutoMap);
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<SignatureInformation> (AutoMap);
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<ParameterInformation> (AutoMap);


    //Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<Models.Company>(cm =>
    //{
    //  cm.AutoMap();
    //  //cm.SetDiscriminator("Company");
    //});
  }

  static partial void PostConfigure()
  {

  }
}
