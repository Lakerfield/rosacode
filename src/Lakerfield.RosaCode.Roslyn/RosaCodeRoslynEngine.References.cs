using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Reflection;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine
  {
    // The framework comes from the reference assemblies, never from the runtime that hosts the application
    private static readonly string[] _neverReference = { "System.Private.CoreLib", "mscorlib", "netstandard" };

    private static IEnumerable<MetadataReference> CreateReferences(RosaCodeRoslynEngineOptions options)
    {
      IEnumerable<Assembly> assemblies;
      if (options.Assemblies != null)
        assemblies = options.Assemblies;
      else if (options.AllowedReferencePrefixes != null)
        assemblies = AppDomain.CurrentDomain.GetAssemblies()
          .Where(a => !a.IsDynamic && a.FullName != null && options.AllowedReferencePrefixes.Any(prefix => a.FullName.StartsWith(prefix)));
      else
        assemblies = RosaCodeRoslynConstants.GetFilteredAppDomainAssemblyReferences();

      var references = new List<MetadataReference>(CreateReferences(assemblies));

      if (options.References != null)
        references.AddRange(options.References);

      return references;
    }

    private static IEnumerable<MetadataReference> CreateReferences(IEnumerable<Assembly> assemblies)
    {
      var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      foreach (var assembly in assemblies)
      {
        if (assembly.IsDynamic || _neverReference.Contains(assembly.GetName().Name))
          continue;

        // empty for assemblies that are loaded from memory or bundled in a single-file application
        var location = assembly.Location;
        if (string.IsNullOrEmpty(location))
        {
          Trace.TraceWarning($"RosaCode: assembly '{assembly.GetName().Name}' has no file location and is not referenced");
          continue;
        }

        if (!locations.Add(location))
          continue;

        MetadataReference reference;
        try
        {
          reference = MetadataReference.CreateFromFile(location);
        }
        catch (Exception ex)
        {
          Trace.TraceWarning($"RosaCode: assembly '{assembly.GetName().Name}' could not be referenced: {ex.Message}");
          continue;
        }

        yield return reference;
      }
    }

    /// <summary>
    /// Completes when the documentation of the .NET reference assemblies is in place (hover and signature help show it).
    /// Already completed when it was on disk, or when <see cref="RosaCodeRoslynEngineOptions.LoadXmlDocumentation"/> is off.
    /// Never faults: offline, the engine keeps working without framework documentation and the next engine tries the download again.
    /// </summary>
    public Task DocumentationLoaded { get; private set; } = Task.CompletedTask;

    private async Task LoadDocumentationAsync(PortableExecutableReference[] baseReferences)
    {
      try
      {
        var withDocumentation = await XmlHelper.GetRefsAsync(baseReferences);

        await RunExclusive(ct =>
        {
          lock (_lock)
          {
            // swap the framework references, keep what was added since
            var replaced = new HashSet<MetadataReference>(baseReferences);
            var references = withDocumentation.Cast<MetadataReference>()
              .Concat(_document.Project.MetadataReferences.Where(r => !replaced.Contains(r)));

            _workspace.TryApplyChanges(_workspace.CurrentSolution.WithProjectMetadataReferences(_document.Project.Id, references));
            _document = _workspace.CurrentSolution.GetDocument(_document.Id)!;
          }

          return Task.FromResult(true);
        }, CancellationToken.None);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.TraceWarning($"RosaCode: could not load the reference documentation: {ex.Message}");
      }
    }

    private static string CreateEditorConfig(RosaCodeRoslynEngineOptions options) => string.Join("\n",
      "root = true",
      "[*.cs]",
      $"indent_style = {(options.UseTabs ? "tab" : "space")}",
      $"indent_size = {options.IndentSize}",
      $"tab_width = {options.IndentSize}",
      "csharp_new_line_before_open_brace = all");

    /// <summary>Adds references to the code of this engine, for example after a plugin was loaded.</summary>
    public Task AddReferences(IEnumerable<Assembly> assemblies, CancellationToken cancellationToken = default)
      => AddReferences(CreateReferences(assemblies).ToList(), cancellationToken);

    /// <summary>Adds references to the code of this engine.</summary>
    public Task AddReferences(IEnumerable<MetadataReference> references, CancellationToken cancellationToken = default)
    {
      var added = references.ToList();

      return RunExclusive(ct =>
      {
        lock (_lock)
        {
          var solution = _workspace.CurrentSolution.AddMetadataReferences(_document.Project.Id, added);
          _workspace.TryApplyChanges(solution);
          _document = _workspace.CurrentSolution.GetDocument(_document.Id)!;
        }

        return Task.FromResult(true);
      }, cancellationToken);
    }
  }
}
