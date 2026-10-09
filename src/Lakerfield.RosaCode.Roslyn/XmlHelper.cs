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

    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

    // The reference assemblies with their XML documentation, as downloaded once into AppData
    private static DirectoryInfo GetRefsDirectory()
    {
      var version = GetRuntimeVersion();

      var rootPath = GetWritableAppDataPath("RosaCode");
      var versionPath = Directory.CreateDirectory(Path.Combine(rootPath, version));
      return new DirectoryInfo(Path.Combine(versionPath.FullName, "ref", GetShortFrameworkName()));
    }

    /// <summary>The folder with the documentation when it was downloaded before, otherwise null. Does not use the network.</summary>
    public static string? TryGetCachedRefPath()
    {
      try
      {
        var refsPath = GetRefsDirectory();
        return refsPath.Exists ? refsPath.FullName : null;
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.TraceWarning($"RosaCode: could not look for the cached reference documentation: {ex.Message}");
        return null;
      }
    }

    private static readonly object _downloadLock = new object();
    private static Task<string>? _download;

    /// <summary>Downloads the documentation when needed. All engines share one download; a failed download is tried again next time.</summary>
    public static Task<string> GetCachedRefPath()
    {
      lock (_downloadLock)
      {
        if (_download == null || _download.IsFaulted || _download.IsCanceled)
          _download = DownloadRefPath();

        return _download;
      }
    }

    private static async Task<string> DownloadRefPath()
    {
      var refsPath = GetRefsDirectory();
      if (refsPath.Exists)
        return refsPath.FullName;

      var version = GetRuntimeVersion();
      var url = $"https://www.nuget.org/api/v2/package/Microsoft.NETCore.App.Ref/{version}";
      var data = await _httpClient.GetByteArrayAsync(url).ConfigureAwait(false);

      // Extract next to the target and move the result into place, so a failure cannot leave half a folder that counts as cached
      var temp = Path.Combine(refsPath.Parent!.Parent!.FullName, "download-" + Guid.NewGuid().ToString("N"));
      try
      {
        using (var archive = new System.IO.Compression.ZipArchive(new MemoryStream(data), System.IO.Compression.ZipArchiveMode.Read))
          archive.ExtractToDirectory(temp);

        Directory.CreateDirectory(refsPath.Parent.FullName);
        try
        {
          Directory.Move(Path.Combine(temp, "ref", GetShortFrameworkName()), refsPath.FullName);
        }
        catch (IOException) when (Directory.Exists(refsPath.FullName))
        {
          // another process was faster
        }
      }
      finally
      {
        try { Directory.Delete(temp, true); } catch { }
      }

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
      catch (Exception ex)
      {
        System.Diagnostics.Trace.TraceWarning($"RosaCode: AppData path unavailable, using temp folder: {ex.Message}");
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



    /// <summary>The references with documentation, when it is already on disk. Does not wait for a download: the references stay as they are then.</summary>
    public static PortableExecutableReference[] GetRefs(PortableExecutableReference[] dllReferences)
    {
      var refPath = TryGetCachedRefPath();
      return refPath == null ? dllReferences : AddXmlDocumentation(dllReferences, refPath);
    }

    /// <summary>The references with documentation. Downloads the documentation first when needed; fails when that is not possible (offline).</summary>
    public static async Task<PortableExecutableReference[]> GetRefsAsync(PortableExecutableReference[] dllReferences)
    {
      var refPath = await GetCachedRefPath().ConfigureAwait(false);
      return AddXmlDocumentation(dllReferences, refPath);
    }

    private static PortableExecutableReference[] AddXmlDocumentation(PortableExecutableReference[] dllReferences, string refPath)
    {
      var references = new List<PortableExecutableReference>();

      foreach (var metadataReference in dllReferences)
        references.AddRange(AddXmlDocumentation(metadataReference, refPath));

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

    private static readonly System.Text.RegularExpressions.Regex _whitespace = new System.Text.RegularExpressions.Regex(@"\s+");

    private static XElement? ParseDocumentation(string? xmlDoc)
    {
      if (string.IsNullOrWhiteSpace(xmlDoc))
        return null;

      try
      {
        var element = XDocument.Parse(xmlDoc).Root;
        return element;
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.TraceWarning($"RosaCode: could not parse XML documentation: {ex.Message}");
        return null;
      }
    }

    /// <summary>The text of a documentation element as plain text/markdown: references (see, paramref) become names, code becomes `code`.</summary>
    private static string Flatten(XNode node)
    {
      switch (node)
      {
        case XText text:
          return text.Value;

        case XElement element:
          switch (element.Name.LocalName)
          {
            case "see":
            case "seealso":
              var cref = element.Attribute("cref")?.Value;
              if (!string.IsNullOrEmpty(cref))
              {
                // "T:System.Collections.Generic.List{T}" -> "List<T>"
                var name = cref.Substring(cref.IndexOf(':') + 1);
                var parenthesis = name.IndexOf('(');
                if (parenthesis > 0) name = name.Substring(0, parenthesis);
                // "List`1" and "Method``1" are generic: leave out the arity, "{T}" is written as "<T>"
                name = System.Text.RegularExpressions.Regex.Replace(name, @"`+\d+", string.Empty).Replace('{', '<').Replace('}', '>');
                return $"`{name.Substring(name.LastIndexOf('.') + 1)}`";
              }
              return element.Attribute("langword")?.Value is { } word ? $"`{word}`" : element.Value;

            case "paramref":
            case "typeparamref":
              return $"`{element.Attribute("name")?.Value}`";

            case "c":
            case "code":
              return $"`{element.Value.Trim()}`";

            case "para":
            case "br":
              return "\n\n" + string.Concat(element.Nodes().Select(Flatten)) + "\n\n";

            default:
              return string.Concat(element.Nodes().Select(Flatten));
          }

        default:
          return string.Empty;
      }
    }

    private static string Text(XElement element)
    {
      var text = string.Concat(element.Nodes().Select(Flatten));

      // keep paragraphs, collapse the layout whitespace of the source
      return string.Join("\n\n", text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
        .Select(paragraph => _whitespace.Replace(paragraph, " ").Trim())
        .Where(paragraph => paragraph.Length > 0));
    }

    /// <summary>Only the summary, as markdown. Null when there is none.</summary>
    public static string? ExtractSummaryOnly(string? xmlDoc)
    {
      var summary = ParseDocumentation(xmlDoc)?.Elements("summary").FirstOrDefault();
      var text = summary == null ? null : Text(summary);
      return string.IsNullOrEmpty(text) ? null : text;
    }

    /// <summary>The documentation of each parameter, by parameter name.</summary>
    public static IReadOnlyDictionary<string, string> ExtractParameterDocs(string? xmlDoc)
    {
      var result = new Dictionary<string, string>();

      var root = ParseDocumentation(xmlDoc);
      if (root == null)
        return result;

      foreach (var param in root.Elements("param"))
      {
        var name = param.Attribute("name")?.Value;
        if (!string.IsNullOrEmpty(name))
          result[name] = Text(param);
      }

      return result;
    }

    /// <summary>Summary, parameters, return value and exceptions as markdown. Null when there is no documentation.</summary>
    public static string? ExtractSummaryFromXml(string? xmlDoc)
    {
      var root = ParseDocumentation(xmlDoc);
      if (root == null)
        return null;

      var result = new StringBuilder();

      var summary = root.Elements("summary").FirstOrDefault();
      if (summary != null)
        result.AppendLine(Text(summary)).AppendLine();

      foreach (var param in root.Elements("param"))
        result.AppendLine($"**{param.Attribute("name")?.Value}** {Text(param)}  ");

      var returns = root.Elements("returns").FirstOrDefault();
      if (returns != null)
        result.AppendLine().AppendLine($"**returns** {Text(returns)}");

      var exceptions = root.Elements("exception").ToList();
      if (exceptions.Count > 0)
      {
        result.AppendLine();
        foreach (var exception in exceptions)
        {
          var cref = exception.Attribute("cref")?.Value ?? string.Empty;
          result.AppendLine($"*{cref.Substring(cref.IndexOf(':') + 1)}* {Text(exception)}  ");
        }
      }

      var text = result.ToString().Trim();
      return text.Length == 0 ? null : text;
    }

  }
}
