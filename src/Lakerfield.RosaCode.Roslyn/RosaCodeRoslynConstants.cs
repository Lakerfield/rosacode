using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lakerfield.RosaCode
{
  public static class RosaCodeRoslynConstants
  {
    /*
     *In project using RosaCode, set allowed prefixes the following way
     
        Lakerfield.RosaCode.RosaCodeRoslynConstants.AllowedReferencePrefixes = new[]{
          "Lakerfield.",
          "Sample.",
        };

     * Or set a custom ReferenceFilter

        Lakerfield.RosaCode.RosaCodeRoslynConstants.ReferenceFilter = (reference) =>
        {
          var prefixes = AllowedReferencePrefixes ?? Array.Empty<string>();
          foreach (var prefix in prefixes)
            if (reference.StartsWith(prefix))
              return true;
          return false;
        };
     */
    public static IEnumerable<Assembly> GetFilteredAppDomainAssemblyReferences()
    {
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        if (assembly.IsDynamic)
          continue;

        if (assembly.FullName == null)
          continue;

        //if (assembly.FullName.StartsWith("System.") ||
        //    assembly.FullName.StartsWith("Microsoft.") ||
        //    assembly.Location.Contains(@"dotnet\shared"))
        //  continue;

        if (!ReferenceFilter(assembly.FullName))
          continue;

        yield return assembly;
      }
    }

    public static Func<string, bool> ReferenceFilter { get; set; } = (reference) =>
    {
      var prefixes = AllowedReferencePrefixes ?? Array.Empty<string>();
      foreach (var prefix in prefixes)
        if (reference.StartsWith(prefix))
          return true;
      return false;
    };

    public static string[]? AllowedReferencePrefixes { get; set; } = {
      "Lakerfield.RosaCode.",
    };



  }
}
