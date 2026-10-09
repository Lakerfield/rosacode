using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine
  {
    /// <summary>
    /// The hints for the visible part of the code: the type of a "var" (and "out var", and a "var" in foreach), and the name of the
    /// parameter in front of an argument that does not say what it is (a literal, null, default, new).
    /// </summary>
    public Task<IReadOnlyList<InlayHintItem>> GetInlayHints(string code, int startLine, int startColumn, int endLine, int endColumn)
      => GetInlayHints(code, startLine, startColumn, endLine, endColumn, CancellationToken.None);

    public Task<IReadOnlyList<InlayHintItem>> GetInlayHints(string code, int startLine, int startColumn, int endLine, int endColumn, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var document = UpdateCode(code);
        var text = await document.GetTextAsync(ct);
        var root = await document.GetSyntaxRootAsync(ct);
        var model = await document.GetSemanticModelAsync(ct);
        if (root == null || model == null)
          return (IReadOnlyList<InlayHintItem>)Array.Empty<InlayHintItem>();

        var start = GetPosition(text, startLine, startColumn);
        var end = GetPosition(text, endLine, endColumn);
        var span = TextSpan.FromBounds(Math.Min(start, end), Math.Max(start, end));

        var hints = new List<InlayHintItem>();
        foreach (var node in root.DescendantNodes(span))
        {
          ct.ThrowIfCancellationRequested();

          switch (node)
          {
            case VariableDeclarationSyntax { Type.IsVar: true } declaration when declaration.Variables.Count == 1:
              AddTypeHint(hints, text, span, model.GetDeclaredSymbol(declaration.Variables[0], ct), declaration.Variables[0].Identifier.Span.End);
              break;

            case ForEachStatementSyntax { Type.IsVar: true } forEach:
              AddTypeHint(hints, text, span, model.GetDeclaredSymbol(forEach, ct), forEach.Identifier.Span.End);
              break;

            case DeclarationExpressionSyntax { Type.IsVar: true, Designation: SingleVariableDesignationSyntax designation }:
              AddTypeHint(hints, text, span, model.GetDeclaredSymbol(designation, ct), designation.Identifier.Span.End);
              break;

            case ArgumentSyntax argument:
              AddParameterHint(hints, text, span, model, argument, ct);
              break;
          }
        }

        return hints;
      }, cancellationToken);

    private static void AddTypeHint(List<InlayHintItem> hints, SourceText text, TextSpan visible, ISymbol? symbol, int position)
    {
      if (symbol is not ILocalSymbol { Type: { } type } || type.TypeKind == TypeKind.Error || type.IsAnonymousType || !visible.Contains(position))
        return;

      var location = text.Lines.GetLinePosition(position);
      hints.Add(new InlayHintItem
      {
        Line = location.Line + 1,
        Column = location.Character + 1,
        Label = ": " + type.ToDisplayString(_typeFormat),
        Kind = "Type",
      });
    }

    private static void AddParameterHint(List<InlayHintItem> hints, SourceText text, TextSpan visible, SemanticModel model, ArgumentSyntax argument, CancellationToken cancellationToken)
    {
      // the argument already says what it is, or only has a name because the call is not finished
      if (argument.NameColon != null || argument.Parent is not ArgumentListSyntax list || !visible.Contains(argument.SpanStart) || !IsUnclear(argument.Expression))
        return;

      var info = model.GetSymbolInfo(list.Parent!, cancellationToken);
      if ((info.Symbol ?? info.CandidateSymbols.FirstOrDefault()) is not IMethodSymbol method)
        return;

      // a method with one parameter says enough with its name
      var index = list.Arguments.IndexOf(argument);
      if (method.Parameters.Length < 2 || index >= method.Parameters.Length)
        return;

      var parameter = method.Parameters[index];
      if (parameter.IsParams && index == method.Parameters.Length - 1)
        return;

      var location = text.Lines.GetLinePosition(argument.SpanStart);
      hints.Add(new InlayHintItem
      {
        Line = location.Line + 1,
        Column = location.Character + 1,
        Label = parameter.Name + ":",
        Kind = "Parameter",
        PaddingRight = true,
      });
    }

    // Literals, null, default and "new" do not say what they are for; names, calls and so on usually do
    private static bool IsUnclear(ExpressionSyntax expression)
    {
      while (expression is CastExpressionSyntax cast)
        expression = cast.Expression;

      return expression is LiteralExpressionSyntax
        or DefaultExpressionSyntax
        or ObjectCreationExpressionSyntax
        or ImplicitObjectCreationExpressionSyntax
        or PrefixUnaryExpressionSyntax { Operand: LiteralExpressionSyntax };
    }
  }
}
