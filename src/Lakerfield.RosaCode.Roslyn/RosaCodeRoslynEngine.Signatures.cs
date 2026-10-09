using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine
  {
    private static readonly SymbolDisplayFormat _parameterFormat = new SymbolDisplayFormat(
      typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
      genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
      parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeDefaultValue,
      miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    private static readonly SymbolDisplayFormat _typeFormat = new SymbolDisplayFormat(
      typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
      genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
      miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public Task<(IEnumerable<SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignatures(string code, int line, int column, CancellationToken cancellationToken)
      => RunExclusive(ct => GetSignaturesCore(code, line, column, ct), cancellationToken);

    private async Task<(IEnumerable<SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignaturesCore(string code, int line, int column, CancellationToken cancellationToken)
    {
      var none = (signatures: (IEnumerable<SignatureItem>)Array.Empty<SignatureItem>(), activeSignature: 0, activeParameter: 0);

      var document = UpdateCode(code);
      var text = await document.GetTextAsync(cancellationToken);
      var position = GetPosition(text, line, column);

      var root = await document.GetSyntaxRootAsync(cancellationToken);
      var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
      if (root == null || semanticModel == null)
        return none;

      var argumentList = FindArgumentList(root, position);
      if (argumentList == null)
        return none;

      var (overloads, resolved) = GetOverloads(semanticModel, argumentList, cancellationToken);
      if (overloads.Count == 0)
        return none;

      // The argument the cursor is in: the number of commas in front of it
      var arguments = argumentList.Arguments;
      var argumentIndex = arguments.GetSeparators().Count(separator => separator.Span.End <= position);

      // The overload the compiler picked; without one the first that has room for this argument
      var activeSignature = resolved == null ? -1 : overloads.FindIndex(m => IsSameMethod(m, resolved));
      if (activeSignature < 0)
        activeSignature = overloads.FindIndex(m => m.Parameters.Length > argumentIndex || (m.Parameters.Length > 0 && m.Parameters[^1].IsParams));
      if (activeSignature < 0)
        activeSignature = 0;

      // A named argument points at its parameter, wherever it is
      var parameters = overloads[activeSignature].Parameters;
      var activeParameter = argumentIndex;
      if (argumentIndex < arguments.Count && arguments[argumentIndex].NameColon is { } nameColon)
      {
        var name = nameColon.Name.Identifier.ValueText;
        for (var i = 0; i < parameters.Length; i++)
          if (parameters[i].Name == name)
            activeParameter = i;
      }
      activeParameter = Math.Min(activeParameter, Math.Max(0, parameters.Length - 1));

      return (overloads.Select(CreateSignature).ToList(), activeSignature, activeParameter);
    }

    // The innermost argument list around the position, also when the call is not finished yet: Foo(a, |
    private static ArgumentListSyntax? FindArgumentList(SyntaxNode root, int position)
    {
      ArgumentListSyntax? best = null;

      foreach (var start in new[] { position, position - 1 })
      {
        if (start < 0)
          continue;

        var token = root.FindToken(start);
        foreach (var list in token.Parent?.AncestorsAndSelf().OfType<ArgumentListSyntax>() ?? Enumerable.Empty<ArgumentListSyntax>())
        {
          var closed = !list.CloseParenToken.IsMissing;
          if (position < list.OpenParenToken.Span.End || (closed && position > list.CloseParenToken.SpanStart))
            continue;

          if (best == null || list.OpenParenToken.SpanStart > best.OpenParenToken.SpanStart)
            best = list;
          break;
        }
      }

      return best;
    }

    // All overloads of the method or constructor that is called, and the one the compiler picked (if any)
    private static (List<IMethodSymbol> overloads, IMethodSymbol? resolved) GetOverloads(SemanticModel semanticModel, ArgumentListSyntax argumentList, CancellationToken cancellationToken)
    {
      var call = argumentList.Parent;
      var symbolInfo = semanticModel.GetSymbolInfo(call!, cancellationToken);
      var resolved = symbolInfo.Symbol as IMethodSymbol;

      var overloads = new List<IMethodSymbol>();
      switch (call)
      {
        case InvocationExpressionSyntax invocation:
          overloads.AddRange(semanticModel.GetMemberGroup(invocation.Expression, cancellationToken).OfType<IMethodSymbol>());
          break;

        case BaseObjectCreationExpressionSyntax creation:
          if (semanticModel.GetTypeInfo(creation, cancellationToken).Type is INamedTypeSymbol type)
            overloads.AddRange(type.InstanceConstructors.Where(c => semanticModel.IsAccessible(creation.SpanStart, c)));
          break;

        case ConstructorInitializerSyntax:
          if (resolved?.ContainingType != null)
            overloads.AddRange(resolved.ContainingType.InstanceConstructors.Where(c => !c.IsImplicitlyDeclared));
          break;
      }

      // delegates, local functions, or code that does not compile yet
      if (overloads.Count == 0)
      {
        if (resolved != null)
          overloads.Add(resolved);
        overloads.AddRange(symbolInfo.CandidateSymbols.OfType<IMethodSymbol>());
      }

      return (overloads.OrderBy(m => m.Parameters.Length).ToList(), resolved);
    }

    private static bool IsSameMethod(IMethodSymbol a, IMethodSymbol b)
      => SymbolEqualityComparer.Default.Equals((a.ReducedFrom ?? a).OriginalDefinition, (b.ReducedFrom ?? b).OriginalDefinition);

    private static SignatureItem CreateSignature(IMethodSymbol method)
    {
      var documentationXml = (method.ReducedFrom ?? method).GetDocumentationCommentXml();
      var parameterDocs = XmlHelper.ExtractParameterDocs(documentationXml);

      // The editor highlights the active parameter by finding its label in the label of the signature
      var parameters = method.Parameters.Select(p => new SignatureItemParameter
      {
        Label = p.ToDisplayString(_parameterFormat),
        Documentation = parameterDocs.TryGetValue(p.Name, out var parameterDoc) ? parameterDoc : string.Empty,
      }).ToList();

      var isConstructor = method.MethodKind == MethodKind.Constructor;
      var name = isConstructor
        ? method.ContainingType.Name
        : method.Name + (method.IsGenericMethod ? $"<{string.Join(", ", method.TypeParameters.Select(t => t.Name))}>" : string.Empty);
      var returnType = isConstructor ? string.Empty : method.ReturnType.ToDisplayString(_typeFormat) + " ";

      return new SignatureItem
      {
        Label = $"{returnType}{name}({string.Join(", ", parameters.Select(p => p.Label))})",
        Documentation = new MarkdownValue(XmlHelper.ExtractSummaryOnly(documentationXml) ?? string.Empty),
        Parameters = parameters,
      };
    }
  }
}
