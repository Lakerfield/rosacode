using Microsoft.CodeAnalysis;
using System.Text;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine
  {
    private static readonly SymbolDisplayFormat _hoverFormat = SymbolDisplayFormat.MinimallyQualifiedFormat
      .AddMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType)
      .WithKindOptions(SymbolDisplayKindOptions.IncludeTypeKeyword | SymbolDisplayKindOptions.IncludeNamespaceKeyword);

    /// <summary>The symbol under the pointer as markdown: its declaration in a code block, followed by its documentation.</summary>
    public Task<string> GetTooltip(string code, int line, int column, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var context = await GetSymbolContext(code, line, column, ct);
        if (context?.Symbol == null)
          return string.Empty;

        // not for the whitespace in front of a token
        var position = GetPosition(context.Text, line, column);
        if (position < context.Token.Span.Start || position > context.Token.Span.End)
          return string.Empty;

        var symbol = context.Symbol;

        var format = symbol is ILocalSymbol or IParameterSymbol or IRangeVariableSymbol
          ? SymbolDisplayFormat.MinimallyQualifiedFormat
          : _hoverFormat;

        var result = new StringBuilder();
        result.AppendLine("```csharp");
        result.AppendLine(KindPrefix(symbol) + symbol.ToDisplayString(format));
        result.Append("```");

        var documentation = XmlHelper.ExtractSummaryFromXml(symbol.GetDocumentationCommentXml(cancellationToken: ct));
        if (documentation != null)
          result.AppendLine().AppendLine().Append(documentation);

        return result.ToString();
      }, cancellationToken);

    private static string KindPrefix(ISymbol symbol) => symbol switch
    {
      ILocalSymbol local => local.IsConst ? "(constant) " : "(local variable) ",
      IParameterSymbol => "(parameter) ",
      IRangeVariableSymbol => "(range variable) ",
      IFieldSymbol field => field.IsConst ? "(constant) " : "(field) ",
      ILabelSymbol => "(label) ",
      _ => string.Empty,
    };
  }
}
