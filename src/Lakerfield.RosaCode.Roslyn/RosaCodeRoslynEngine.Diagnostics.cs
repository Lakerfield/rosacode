using Microsoft.CodeAnalysis;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine
  {
    private const int MaxDiagnostics = 500;

    public Task<IEnumerable<ActionDiagnostic>> GetDiagnostics(string code, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var document = UpdateCode(code);

        var semanticModel = await document.GetSemanticModelAsync(ct);
        if (semanticModel == null)
          return Enumerable.Empty<ActionDiagnostic>();

        return (IEnumerable<ActionDiagnostic>)semanticModel.GetDiagnostics(cancellationToken: ct)
          .Where(IsShown)
          .Take(MaxDiagnostics)
          .Select(Convert)
          .ToList();
      }, cancellationToken);

    private static bool IsShown(Diagnostic diagnostic)
    {
      if (!diagnostic.Location.IsInSource)
        return false;

      if (diagnostic.Severity != DiagnosticSeverity.Hidden)
        return true;

      // Hidden diagnostics are only interesting as faded out code, for example an unused using
      return GetTags(diagnostic).Contains(WellKnownDiagnosticTags.Unnecessary);
    }

    // The compiler reports an unused using (CS8019) as hidden without the tag that Visual Studio adds to fade it out
    private static string[] GetTags(Diagnostic diagnostic)
    {
      var tags = diagnostic.Descriptor.CustomTags.ToList();
      if (diagnostic.Id == "CS8019" && !tags.Contains(WellKnownDiagnosticTags.Unnecessary))
        tags.Add(WellKnownDiagnosticTags.Unnecessary);

      return tags.ToArray();
    }

    public ActionDiagnostic Convert(Diagnostic diagnostic)
    {
      var location = diagnostic.Location.GetLineSpan();

      return new ActionDiagnostic
      {
        Message = diagnostic.GetMessage(),
        StartLineNumber = location.StartLinePosition.Line + 1,
        StartColumn = location.StartLinePosition.Character + 1,
        EndLineNumber = location.EndLinePosition.Line + 1,
        EndColumn = location.EndLinePosition.Character + 1,
        Severity = diagnostic.Severity.ToString(),
        Id = diagnostic.Id,
        HelpLink = diagnostic.Descriptor.HelpLinkUri ?? string.Empty,
        Tags = GetTags(diagnostic),
      };
    }
  }
}
