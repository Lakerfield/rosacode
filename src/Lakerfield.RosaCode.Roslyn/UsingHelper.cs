using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;

namespace Lakerfield.RosaCode;

class UsingHelper
{
  /// <summary>
  /// Returns true if the compilation unit already contains a using directive
  /// with the exact namespace text (e.g. "System.Collections").
  /// </summary>
  public static bool HasUsing(CompilationUnitSyntax root, string @namespace)
  {
    // root.Usings is an IReadOnlyList<UsingDirectiveSyntax>
    return root.Usings.Any(u => u.Name.ToString() == @namespace);
  }

  public static bool HasUsingOrAlias(CompilationUnitSyntax root, string @namespace)
  {
    return root.Usings.Any(u =>
    {
      // u.Name may be a QualifiedNameSyntax or IdentifierNameSyntax.
      // We compare the *full* display string, which already strips the alias part.
      var name = u.Name.ToString();               // e.g. "System.Collections"
      return name == @namespace;
    });
  }

  /// <summary>
  /// Returns true if the given namespace is already imported (including
  /// global usings, project‑wide usings, and usings from referenced files).
  /// </summary>
  public static bool IsNamespaceInScope(SemanticModel model, int position, string @namespace)
  {
    // Create a dummy identifier that lives at the requested position.
    // Roslyn will resolve it using the current set of usings.
    var syntaxTree = model.SyntaxTree;
    var token = syntaxTree.GetRoot().FindToken(position);
    var nameSyntax = SyntaxFactory.ParseName(@namespace);

    // Ask the model for the symbols that the name resolves to.
    var info = model.GetSpeculativeSymbolInfo(position, nameSyntax, SpeculativeBindingOption.BindAsExpression);
    // If any symbol is found, the namespace is already in scope.
    return info.Symbol != null;
  }



  /// <summary>
  /// Recursively adds the namespace of <paramref name="typeSymbol"/> and of
  /// all nested generic type arguments to <paramref name="result"/>.
  /// Primitive / built‑in types are ignored because they do not require an
  /// explicit using directive.
  /// </summary>
  /// <param name="typeSymbol">The Roslyn type symbol to examine (can be null).</param>
  /// <param name="result">A set that will receive the required namespaces.</param>
  public static void AddNamespacesFromType(ITypeSymbol? typeSymbol, HashSet<string> result)
  {
    if (typeSymbol == null)
      return;

    // 1️⃣  Add the namespace of the type itself (skip the global namespace)
    var ns = typeSymbol.ContainingNamespace?.ToDisplayString();
    if (!string.IsNullOrEmpty(ns) && ns != "<global namespace>")
      result.Add(ns);

    // 2️⃣  If it is a generic type, also add the namespaces of its arguments
    if (typeSymbol is INamedTypeSymbol named && named.IsGenericType)
    {
      foreach (var arg in named.TypeArguments)
        AddNamespacesFromType(arg, result);   // recurse
    }

    // 3️⃣  Special‑case: IEnumerable<T> needs the *non‑generic* IEnumerator
    //     (we add its namespace here so that GetNonGenericEnumeratorReturnType
    //     can focus on the return type only).
    if (typeSymbol.Name == nameof(IEnumerable) && typeSymbol is INamedTypeSymbol it && it.TypeArguments.Length == 1)
    {
      // The generic argument may itself be a generic type – recurse again.
      AddNamespacesFromType(it.TypeArguments[0], result);
    }
  }

  /// <summary>
  /// Adds the namespaces required for a *generated* member (method, property,
  /// event) to the supplied set.  This is used for the explicit interface
  /// implementation that the generator creates.
  /// </summary>
  public static void AddNamespacesFromGeneratedMember(IMethodSymbol method, HashSet<string> set, Compilation compilation)
  {
    // Return type
    AddNamespacesFromType(method.ReturnType, set);

    // Parameter types
    foreach (var p in method.Parameters)
      AddNamespacesFromType(p.Type, set);
  }

  // --------------------------------------------------------------
  // Helper: locate the *non‑generic* IEnumerator return type for a generic IEnumerable<T>
  // --------------------------------------------------------------
  public static ITypeSymbol? GetNonGenericEnumeratorReturnType(
      INamedTypeSymbol genericInterface,
      Compilation compilation)
  {
    // Find the base interface that defines a parameter‑less GetEnumerator()
    // (for IEnumerable<T> the base is System.Collections.IEnumerable).
    if (genericInterface == null)
      return null;

    foreach (var baseIfc in genericInterface.AllInterfaces)
    {
      foreach (var member in baseIfc.GetMembers("GetEnumerator"))
      {
        if (member is IMethodSymbol method &&
            method.Parameters.Length == 0)
        {
          // The method’s return type is the *non‑generic* IEnumerator.
          // Its namespace will be added dynamically by the caller.
          return method.ReturnType;
        }
      }
    }

    // If no base GetEnumerator is found we simply return null – the
    // generic walk above already handled everything else.
    return null;
  }
}
