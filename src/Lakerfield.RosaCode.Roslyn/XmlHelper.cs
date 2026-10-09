using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Versioning;

namespace Lakerfield.RosaCode
{
  class XmlHelper
  {

    public static async Task<string> GetCachedRefPath()
    {
      var version = GetRuntimeVersion();

      var rootPath = GetWritableAppDataPath("RosaCode");
      var versionPath = Directory.CreateDirectory(Path.Combine(rootPath, version));
      var refsPath = new DirectoryInfo(Path.Combine(versionPath.FullName, "ref", GetShortFrameworkName()));
      if (refsPath.Exists)
        return refsPath.FullName;

      var httpClient = new HttpClient();
      var url = $"https://www.nuget.org/api/v2/package/Microsoft.NETCore.App.Ref/{version}";
      var data = await httpClient.GetByteArrayAsync(url);
      var archive = new System.IO.Compression.ZipArchive(new MemoryStream(data), System.IO.Compression.ZipArchiveMode.Read);
      archive.ExtractToDirectory(versionPath.FullName);

      return refsPath.FullName;
    }

    /// <summary>
    /// Returns the full .NET runtime version, like "8.0.12".
    /// </summary>
    public static string GetRuntimeVersion()
    {
      // FrameworkDescription gives us something like ".NET 8.0.12"
      var desc = RuntimeInformation.FrameworkDescription;
      if (desc.StartsWith(".NET "))
        return desc.Replace(".NET ", "").Trim();

      return desc.Trim();
    }

    public static string GetWritableAppDataPath(string appName)
    {
      try
      {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var path = Path.Combine(basePath, appName);
        Directory.CreateDirectory(path);
        return path;
      }
      catch
      {
        var fallback = Path.Combine(Path.GetTempPath(), appName);
        Directory.CreateDirectory(fallback);
        return fallback;
      }
    }

    public static string? GetShortFrameworkName()
    {
      var attr = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>();
      if (attr == null) return "unknown";

      var parsed = attr.FrameworkName; // e.g., ".NETCoreApp,Version=v8.0"
      if (parsed.StartsWith(".NETCoreApp,Version=v"))
        return "net" + parsed.Replace(".NETCoreApp,Version=v", "");

      var version = GetRuntimeVersion();
      var parts = version.Split('.');
      return $"net{parts[0]}.{parts[1]}";
    }



    public static PortableExecutableReference[] GetRefs(PortableExecutableReference[] dllReferences)
    {
      List<PortableExecutableReference> references = new List<PortableExecutableReference>();

      var refPath = Task.Run(() => GetCachedRefPath()).Result;

      foreach (var metadataReference in dllReferences)
      {
        var xmlReferences = AddXmlDocumentation(metadataReference, refPath);

        foreach (var reference in xmlReferences)
          references.Add(reference);
      }

      return references.ToArray();
    }

    private static PortableExecutableReference[] AddXmlDocumentation(PortableExecutableReference reference, string refPath)
    {
      string dllPath = Path.Combine(refPath, Path.GetFileName(reference.FilePath));

      //string dllPath = reference.FilePath;
      string xmlPath = Path.ChangeExtension(dllPath, ".xml");

      List<PortableExecutableReference> references = new List<PortableExecutableReference>();

      if (File.Exists(xmlPath))
      {
        references.Add(MetadataReference.CreateFromFile(dllPath, documentation: XmlDocumentationProvider.CreateFromFile(xmlPath)));
        //return reference.WithXmlDocumentationProvider(XmlDocumentationProvider.CreateFromFile(xmlPath));
      }
      else if (File.Exists(dllPath))
        references.Add(MetadataReference.CreateFromFile(dllPath));
      else
        references.Add(reference);

      return references.ToArray();
    }

    public static string ExtractSummaryFromXml(string xmlDoc)
    {
      if (string.IsNullOrEmpty(xmlDoc))
        return null;

      try
      {
        var doc = XDocument.Parse(xmlDoc);
        var result = new StringBuilder();
        var hasException = false;
        foreach (var node in doc.Descendants())
        {
          switch (node.Name.LocalName)
          {
            case "summary":
              result.AppendLine(node.Value);
              result.AppendLine();
              break;

            case "param":
              result.AppendLine($"**{node.Attribute("name")?.Value}** {node.Value}  ");
              break;

            case "exception":
              if (!hasException)
              {
                hasException = true;
                result.AppendLine();
              }
              result.AppendLine($"*{node.Attribute("cref")?.Value}* {node.Value}  ");
              break;

            case "returns":
              result.AppendLine();
              result.AppendLine($"**returns** {node.Value}<br>");
              break;

            case "member":
              break;

            default:
              result.AppendLine($"{node.Name.LocalName}: {node.Value}");
              break;
          }
        }
        return result.ToString();
      }
      catch (Exception ex)
      {
        return null;
      }
    }

  }
}
