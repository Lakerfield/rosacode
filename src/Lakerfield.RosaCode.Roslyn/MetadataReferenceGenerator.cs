using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lakerfield.RosaCode
{
  public class MetadataReferenceGenerator
  {


    private static object _metadataReferencesLock = new object();
    private static MetadataReference[]? _metadataReferences;
    public static MetadataReference[] GenerateMetadataReferences()
    {
      if (_metadataReferences != null)
        return _metadataReferences;

      lock (_metadataReferencesLock)
      {
        System.Threading.Thread.MemoryBarrier();
        if (_metadataReferences != null)
          return _metadataReferences;

        var metadataReferences = new List<MetadataReference>();

        //foreach (var assembly in PluginConstants.GetFilteredAppDomainAssemblyReferences())
        //{
        //  metadataReferences.Add(MetadataReference.CreateFromFile(assembly.Location));
        //}
        _metadataReferences = metadataReferences.ToArray();
      }
      return _metadataReferences;
    }


  }
}
