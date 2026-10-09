using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Reflection;

namespace Lakerfield.RosaCode
{
  /// <summary>
  /// Runs the code fix and refactoring providers that ship with Roslyn (Microsoft.CodeAnalysis.*.Features),
  /// the same ones Visual Studio uses for Ctrl+. (add using, implement interface, generate method, extract method, ...).
  /// </summary>
  internal class RoslynCodeActionProvider
  {
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private static readonly Lazy<CodeFixProvider[]> _codeFixProviders = new(() =>
      LoadProviders<CodeFixProvider>(type => type.GetCustomAttribute<ExportCodeFixProviderAttribute>()?.Languages));

    private static readonly Lazy<CodeRefactoringProvider[]> _refactoringProviders = new(() =>
      LoadProviders<CodeRefactoringProvider>(type => type.GetCustomAttribute<ExportCodeRefactoringProviderAttribute>()?.Languages));

    // One MEF container for the workspace and the providers, so they share the services the providers depend on.
    private static readonly Lazy<CompositionHost> _container = new(() =>
      new ContainerConfiguration().WithAssemblies(MefHostServices.DefaultAssemblies).CreateContainer());

    public static MefHostServices Host { get; } = CreateHost();

    private static MefHostServices CreateHost()
    {
      try
      {
        return MefHostServices.Create(_container.Value);
      }
      catch (Exception ex)
      {
        Trace.TraceError($"RosaCode: could not create the MEF host, using the default one: {ex}");
        return MefHostServices.DefaultHost;
      }
    }

    private static T[] LoadProviders<T>(Func<Type, string[]?> getLanguages) where T : class
    {
      try
      {
        return _container.Value.GetExports<T>()
          .Where(provider => getLanguages(provider.GetType())?.Contains(LanguageNames.CSharp) == true)
          .ToArray();
      }
      catch (Exception ex)
      {
        Trace.TraceError($"RosaCode: could not load {typeof(T).Name}s: {ex}");
        return Array.Empty<T>();
      }
    }

    /// <summary>Loads the providers in the background, so the first Ctrl+. is not slow.</summary>
    public static void Preload()
    {
      _ = _codeFixProviders.Value;
      _ = _refactoringProviders.Value;
    }

    public class Result
    {
      public CodeAction Action { get; init; } = null!;
      public bool IsFix { get; init; }
      public ImmutableArray<Diagnostic> Diagnostics { get; init; } = ImmutableArray<Diagnostic>.Empty;
    }

    /// <summary>All code fixes (for the compiler diagnostics at the span) and refactorings available at the span.</summary>
    public async Task<IReadOnlyList<Result>> GetActionsAsync(Document document, TextSpan span, CancellationToken requestToken = default)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(requestToken);
      cts.CancelAfter(Timeout);
      var cancellationToken = cts.Token;
      var results = new List<Result>();

      // Code fixes
      var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
      var diagnostics = semanticModel?.GetDiagnostics(cancellationToken: cancellationToken)
        .Where(d => d.Location.IsInSource && (d.Location.SourceSpan.IntersectsWith(span) || d.Location.SourceSpan.Contains(span)))
        ?? Enumerable.Empty<Diagnostic>();

      foreach (var diagnostic in diagnostics)
      {
        foreach (var provider in _codeFixProviders.Value)
        {
          if (!provider.FixableDiagnosticIds.Contains(diagnostic.Id))
            continue;

          try
          {
            var actions = new List<(CodeAction, ImmutableArray<Diagnostic>)>();
            var context = new CodeFixContext(document, diagnostic, (action, diags) => { lock (actions) actions.Add((action, diags)); }, cancellationToken);
            await provider.RegisterCodeFixesAsync(context);

            foreach (var (action, diags) in actions)
              results.Add(new Result { Action = action, IsFix = true, Diagnostics = diags });
          }
          catch (Exception ex) when (ex is not OperationCanceledException)
          {
            Trace.TraceWarning($"RosaCode: code fix provider {provider.GetType().Name} failed: {ex.Message}");
          }
        }
      }

      // Refactorings
      foreach (var provider in _refactoringProviders.Value)
      {
        try
        {
          var actions = new List<CodeAction>();
          var context = new CodeRefactoringContext(document, span, action => { lock (actions) actions.Add(action); }, cancellationToken);
          await provider.ComputeRefactoringsAsync(context);

          foreach (var action in actions)
            results.Add(new Result { Action = action, IsFix = false });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
          Trace.TraceWarning($"RosaCode: refactoring provider {provider.GetType().Name} failed: {ex.Message}");
        }
      }

      return results;
    }
  }
}
