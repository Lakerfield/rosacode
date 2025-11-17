using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lakerfield.RosaCode;

static class GenericImplHelpers
{
  // -----------------------------------------------------------------
  // 1️⃣ Add missing using directives
  // -----------------------------------------------------------------
  public static CompilationUnitSyntax AddMissingUsings(
      CompilationUnitSyntax root,
      IEnumerable<string> neededNamespaces)
  {
    var missing = neededNamespaces
        .Where(ns => !string.IsNullOrEmpty(ns))
        .Except(root.Usings.Select(u => u.Name.ToString()))
        .Distinct();

    if (!missing.Any())
      return root;

    var newUsings = missing.Select(ns =>
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns))
                     .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));

    return root.WithUsings(SyntaxFactory.List(root.Usings.Concat(newUsings)));
  }

  // -----------------------------------------------------------------
  // 2️⃣ Recursively collect namespaces needed for a symbol
  // -----------------------------------------------------------------
  public static IEnumerable<string> GetNamespacesForSymbol(ISymbol symbol)
  {
    // Minimally‑qualified name (e.g. System.Collections.Generic)
    var format = SymbolDisplayFormat.FullyQualifiedFormat
        .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)
        .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    var fullName = symbol.ToDisplayString(format);
    var lastDot = fullName.LastIndexOf('.');
    if (lastDot > 0)
    {
      var ns = fullName.Substring(0, lastDot);
      yield return ns;
    }

    // Recurse into generic type arguments
    if (symbol is INamedTypeSymbol named && named.IsGenericType)
    {
      foreach (var arg in named.TypeArguments)
        foreach (var ns in GetNamespacesForSymbol(arg))
          yield return ns;
    }

    // Walk method/property/event signatures
    switch (symbol)
    {
      case IMethodSymbol m:
        foreach (var p in m.Parameters)
          foreach (var ns in GetNamespacesForSymbol(p.Type))
            yield return ns;
        foreach (var ns in GetNamespacesForSymbol(m.ReturnType))
          yield return ns;
        break;

      case IPropertySymbol p:
        foreach (var ns in GetNamespacesForSymbol(p.Type))
          yield return ns;
        break;

      case IEventSymbol e:
        foreach (var ns in GetNamespacesForSymbol(e.Type))
          yield return ns;
        break;

      case IFieldSymbol f:
        foreach (var ns in GetNamespacesForSymbol(f.Type))
          yield return ns;
        break;
    }
  }

  // -----------------------------------------------------------------
  // 3️⃣ Merge (or create) class type‑parameter list so it contains the
  //    type parameters required by the interface.
  // -----------------------------------------------------------------
  public static ClassDeclarationSyntax MergeClassTypeParameters(
      ClassDeclarationSyntax classNode,
      INamedTypeSymbol interfaceSymbol)
  {
    var existing = classNode.TypeParameterList?.Parameters
                     .Select(p => p.Identifier.ValueText)
                     .ToHashSet() ?? new HashSet<string>();

    var toAdd = new List<TypeParameterSyntax>();
    foreach (var ifaceTp in interfaceSymbol.TypeParameters)
    {
      if (!existing.Contains(ifaceTp.Name))
      {
        var tp = SyntaxFactory.TypeParameter(SyntaxFactory.Identifier(ifaceTp.Name));
        toAdd.Add(tp);
        existing.Add(ifaceTp.Name);
      }
    }

    if (!toAdd.Any())
      return classNode;

    var newList = classNode.TypeParameterList == null
        ? SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(toAdd))
        : classNode.TypeParameterList.AddParameters(toAdd.ToArray());

    return classNode.WithTypeParameterList(newList);
  }

  // -----------------------------------------------------------------
  // 4️⃣ Build a SimpleBaseTypeSyntax for the (possibly generic) interface.
  // -----------------------------------------------------------------
  public static SimpleBaseTypeSyntax BuildGenericBase(
      INamedTypeSymbol iface,
      IEnumerable<ITypeSymbol> typeArguments)
  {
    // If the interface is generic we need a GenericName with a TypeArgumentList.
    if (iface.IsGenericType)
    {
      var genericName = SyntaxFactory.GenericName(
          SyntaxFactory.Identifier(iface.Name))
          .WithTypeArgumentList(
              SyntaxFactory.TypeArgumentList(
                  SyntaxFactory.SeparatedList<TypeSyntax>(
                      typeArguments.Select(t =>
                          SyntaxFactory.ParseTypeName(
                              t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))))));

      return SyntaxFactory.SimpleBaseType(genericName);
    }

    // Non‑generic case
    return SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(iface.Name));
  }

  // -----------------------------------------------------------------
  // 5️⃣ Generate a stub for a method (including generic signatures)
  // -----------------------------------------------------------------
  public static MethodDeclarationSyntax GenerateMethodStub(
      IMethodSymbol method,
      SemanticModel model)
  {
    // Return type
    var returnType = SyntaxFactory.ParseTypeName(
        method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

    // Method declaration
    var decl = SyntaxFactory.MethodDeclaration(returnType, method.Name);

    // Modifiers – explicit impl gets no accessibility keyword
    var modifiers = method.ExplicitInterfaceImplementations.Any()
        ? SyntaxFactory.TokenList()
        : SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

    decl = decl.WithModifiers(modifiers);

    // Parameters (preserve ref/out/in and default values)
    var parameters = method.Parameters.Select(p =>
    {
      var type = SyntaxFactory.ParseTypeName(
          p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

      var param = SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
          .WithType(type);

      // ref/out/in
      if (p.RefKind == RefKind.Ref)
        param = param.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword)));
      else if (p.RefKind == RefKind.Out)
        param = param.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.OutKeyword)));
      else if (p.RefKind == RefKind.In)
        param = param.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InKeyword)));

      // default value (if any)
      if (p.HasExplicitDefaultValue)
        param = param.WithDefault(
            SyntaxFactory.EqualsValueClause(
                SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)));

      return param;
    });

    decl = decl.WithParameterList(
        SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)));

    // Body – throw NotImplementedException
    var throwStmt = SyntaxFactory.ThrowStatement(
        SyntaxFactory.ObjectCreationExpression(
            SyntaxFactory.ParseTypeName("System.NotImplementedException"))
        .WithArgumentList(SyntaxFactory.ArgumentList()));

    decl = decl.WithBody(SyntaxFactory.Block(throwStmt));

    // Explicit interface qualifier (if needed)
    if (method.ExplicitInterfaceImplementations.Any())
    {
      var iface = method.ExplicitInterfaceImplementations.First().ContainingType;
      decl = decl.WithExplicitInterfaceSpecifier(
          SyntaxFactory.ExplicitInterfaceSpecifier(
              SyntaxFactory.IdentifierName(iface.Name)));
    }

    return decl;
  }

  // -----------------------------------------------------------------
  // 6️⃣ Generate a stub for a property
  // -----------------------------------------------------------------
  public static PropertyDeclarationSyntax GeneratePropertyStub(IPropertySymbol prop)
  {
    var type = SyntaxFactory.ParseTypeName(
        prop.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

    var accessors = new List<AccessorDeclarationSyntax>();

    if (prop.GetMethod != null)
    {
      var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
          .WithBody(SyntaxFactory.Block(
              SyntaxFactory.ThrowStatement(
                  SyntaxFactory.ObjectCreationExpression(
                      SyntaxFactory.ParseTypeName("System.NotImplementedException"))
                  .WithArgumentList(SyntaxFactory.ArgumentList()))));
      accessors.Add(getter);
    }

    if (prop.SetMethod != null)
    {
      var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
          .WithBody(SyntaxFactory.Block(
              SyntaxFactory.ThrowStatement(
                  SyntaxFactory.ObjectCreationExpression(
                      SyntaxFactory.ParseTypeName("System.NotImplementedException"))
                  .WithArgumentList(SyntaxFactory.ArgumentList()))));
      accessors.Add(setter);
    }

    var propDecl = SyntaxFactory.PropertyDeclaration(type, prop.Name)
        .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(accessors)));

    // Explicit interface implementation?
    if (prop.ExplicitInterfaceImplementations.Any())
    {
      var iface = prop.ExplicitInterfaceImplementations.First().ContainingType;
      propDecl = propDecl.WithExplicitInterfaceSpecifier(
          SyntaxFactory.ExplicitInterfaceSpecifier(
              SyntaxFactory.IdentifierName(iface.Name)));
    }
    else
    {
      // Public if not explicit
      propDecl = propDecl.WithModifiers(
          SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
    }

    return propDecl;
  }

  // -----------------------------------------------------------------
  // 7️⃣ Generate a stub for an event
  // -----------------------------------------------------------------
  public static EventDeclarationSyntax GenerateEventStub(IEventSymbol ev)
  {
    var type = SyntaxFactory.ParseTypeName(
        ev.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

    var add = SyntaxFactory.AccessorDeclaration(SyntaxKind.AddAccessorDeclaration)
        .WithBody(SyntaxFactory.Block(
            SyntaxFactory.ThrowStatement(
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName("System.NotImplementedException"))
                .WithArgumentList(SyntaxFactory.ArgumentList()))));

    var remove = SyntaxFactory.AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration)
        .WithBody(SyntaxFactory.Block(
            SyntaxFactory.ThrowStatement(
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName("System.NotImplementedException"))
                .WithArgumentList(SyntaxFactory.ArgumentList()))));

    var evDecl = SyntaxFactory.EventDeclaration(type, ev.Name)
        .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[] { add, remove })));

    // Explicit interface?
    if (ev.ExplicitInterfaceImplementations.Any())
    {
      var iface = ev.ExplicitInterfaceImplementations.First().ContainingType;
      evDecl = evDecl.WithExplicitInterfaceSpecifier(
          SyntaxFactory.ExplicitInterfaceSpecifier(
              SyntaxFactory.IdentifierName(iface.Name)));
    }
    else
    {
      evDecl = evDecl.WithModifiers(
          SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
    }

    return evDecl;
  }

  // -----------------------------------------------------------------
  // 8️⃣ Gather **all namespaces** required by the interface and its members.
  // -----------------------------------------------------------------
  public static IEnumerable<string> GetAllRequiredNamespaces(INamedTypeSymbol iface)
  {
    var result = new HashSet<string>();

    // Interface's own namespace
    if (!iface.ContainingNamespace.IsGlobalNamespace)
      result.Add(iface.ContainingNamespace.ToDisplayString());

    // Walk members (return type + parameter types + constraints)
    foreach (var member in iface.GetMembers())
    {
      result.UnionWith(GetNamespacesForSymbol(member));

      if (member is IMethodSymbol m)
      {
        foreach (var p in m.Parameters)
          result.UnionWith(GetNamespacesForSymbol(p.Type));
        result.UnionWith(GetNamespacesForSymbol(m.ReturnType));
      }
      else if (member is IPropertySymbol p)
        result.UnionWith(GetNamespacesForSymbol(p.Type));
      else if (member is IEventSymbol e)
        result.UnionWith(GetNamespacesForSymbol(e.Type));
    }

    return result;
  }

  // -----------------------------------------------------------------
  // 9️⃣ Recursively collect namespaces for a symbol (used above)
  // -----------------------------------------------------------------
  private static IEnumerable<string> GetNamespacesForSymbol2(ISymbol symbol)
  {
    if (symbol == null) yield break;

    var full = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    var lastDot = full.LastIndexOf('.');
    if (lastDot > 0)
    {
      var ns = full.Substring(0, lastDot);
      if (!string.IsNullOrWhiteSpace(ns))
        yield return ns;
    }

    // Recurse into generic type arguments
    if (symbol is INamedTypeSymbol named && named.IsGenericType)
    {
      foreach (var arg in named.TypeArguments)
        foreach (var ns in GetNamespacesForSymbol(arg))
          yield return ns;
    }

    // Recurse into method signatures
    if (symbol is IMethodSymbol m)
    {
      foreach (var p in m.Parameters)
        foreach (var ns in GetNamespacesForSymbol(p.Type))
          yield return ns;

      foreach (var ns in GetNamespacesForSymbol(m.ReturnType))
        yield return ns;
    }
    else if (symbol is IPropertySymbol p)
    {
      foreach (var ns in GetNamespacesForSymbol(p.Type))
        yield return ns;
    }
    else if (symbol is IEventSymbol e)
    {
      foreach (var ns in GetNamespacesForSymbol(e.Type))
        yield return ns;
    }
  }
}
