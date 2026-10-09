using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Lakerfield.RosaCode
{
  /// <summary>Settings of one <see cref="RosaCodeRoslynEngine"/>. Every engine has its own references.</summary>
  public class RosaCodeRoslynEngineOptions
  {
    public string Name { get; set; } = "default";

    /// <summary>The initial code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Indentation of the code that code actions generate. The editor of the control uses 2 spaces.</summary>
    public int IndentSize { get; set; } = 2;

    public bool UseTabs { get; set; }

    /// <summary>
    /// Assemblies the code can use, on top of the .NET reference assemblies.
    /// When null, the assemblies of the running application that match <see cref="AllowedReferencePrefixes"/> are used.
    /// </summary>
    public IEnumerable<Assembly>? Assemblies { get; set; }

    /// <summary>Extra references, for example from a file or a stream.</summary>
    public IEnumerable<MetadataReference>? References { get; set; }

    /// <summary>
    /// Used when <see cref="Assemblies"/> is null: the assemblies of the running application whose full name starts with one of these.
    /// When null, <see cref="RosaCodeRoslynConstants.AllowedReferencePrefixes"/> (or its <see cref="RosaCodeRoslynConstants.ReferenceFilter"/>) is used.
    /// </summary>
    public IReadOnlyList<string>? AllowedReferencePrefixes { get; set; }

    /// <summary>
    /// Download the XML documentation of the .NET reference assemblies (once, cached in AppData), for hover and signature help.
    /// Without it, or when the download fails, the editor works without framework documentation.
    /// </summary>
    public bool LoadXmlDocumentation { get; set; } = true;
  }
}
