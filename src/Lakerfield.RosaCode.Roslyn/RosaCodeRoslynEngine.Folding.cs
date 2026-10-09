using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine
  {
    /// <summary>
    /// The parts that can be folded: everything between braces, #region blocks, the using directives, and block comments.
    /// Syntax only, so it also works while the code does not compile.
    /// </summary>
    public Task<IReadOnlyList<FoldingRangeItem>> GetFoldingRanges(string code)
      => GetFoldingRanges(code, CancellationToken.None);

    public Task<IReadOnlyList<FoldingRangeItem>> GetFoldingRanges(string code, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var document = UpdateCode(code);
        var text = await document.GetTextAsync(ct);
        var root = await document.GetSyntaxRootAsync(ct);
        if (root == null)
          return (IReadOnlyList<FoldingRangeItem>)Array.Empty<FoldingRangeItem>();

        var ranges = new List<FoldingRangeItem>();

        // Braces: the line with the header stays, the closing brace stays too
        foreach (var node in root.DescendantNodes())
        {
          ct.ThrowIfCancellationRequested();

          var (open, close) = GetBraces(node);
          if (open.IsKind(SyntaxKind.None) || close.IsKind(SyntaxKind.None) || close.IsMissing)
            continue;

          // with the brace on its own line, the header is the line in front of it
          var header = open.GetPreviousToken();
          var startLine = text.Lines.GetLineFromPosition(header.IsKind(SyntaxKind.None) ? open.SpanStart : header.Span.End).LineNumber;
          var endLine = text.Lines.GetLineFromPosition(close.SpanStart).LineNumber - 1;

          if (endLine > startLine)
            ranges.Add(new FoldingRangeItem { StartLine = startLine + 1, EndLine = endLine + 1 });
        }

        // The using directives together
        var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().Where(u => u.Parent is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax).ToList();
        foreach (var group in usings.GroupBy(u => u.Parent))
        {
          var first = text.Lines.GetLineFromPosition(group.First().SpanStart).LineNumber;
          var last = text.Lines.GetLineFromPosition(group.Last().Span.End).LineNumber;
          if (last > first)
            ranges.Add(new FoldingRangeItem { StartLine = first + 1, EndLine = last + 1, Kind = "Imports" });
        }

        // Block comments, documentation comments, #region
        var regions = new Stack<int>();
        foreach (var trivia in root.DescendantTrivia())
        {
          switch (trivia.Kind())
          {
            case SyntaxKind.MultiLineCommentTrivia:
            case SyntaxKind.MultiLineDocumentationCommentTrivia:
            case SyntaxKind.SingleLineDocumentationCommentTrivia:
              var lines = text.Lines.GetLinePositionSpan(trivia.Span);
              if (lines.End.Line > lines.Start.Line)
                ranges.Add(new FoldingRangeItem { StartLine = lines.Start.Line + 1, EndLine = lines.End.Line + 1, Kind = "Comment" });
              break;

            case SyntaxKind.RegionDirectiveTrivia:
              regions.Push(text.Lines.GetLineFromPosition(trivia.SpanStart).LineNumber);
              break;

            case SyntaxKind.EndRegionDirectiveTrivia when regions.Count > 0:
              var regionStart = regions.Pop();
              var regionEnd = text.Lines.GetLineFromPosition(trivia.SpanStart).LineNumber;
              if (regionEnd > regionStart)
                ranges.Add(new FoldingRangeItem { StartLine = regionStart + 1, EndLine = regionEnd + 1, Kind = "Region" });
              break;
          }
        }

        return ranges;
      }, cancellationToken);

    // The braces that belong to a node that can be folded
    private static (SyntaxToken open, SyntaxToken close) GetBraces(SyntaxNode node) => node switch
    {
      BaseTypeDeclarationSyntax type => (type.OpenBraceToken, type.CloseBraceToken),
      BaseNamespaceDeclarationSyntax @namespace when @namespace is NamespaceDeclarationSyntax block => (block.OpenBraceToken, block.CloseBraceToken),
      BlockSyntax block when block.Parent is not (AccessorDeclarationSyntax or MethodDeclarationSyntax or ConstructorDeclarationSyntax or DestructorDeclarationSyntax or OperatorDeclarationSyntax or ConversionOperatorDeclarationSyntax or LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax) => (block.OpenBraceToken, block.CloseBraceToken),
      BaseMethodDeclarationSyntax { Body: { } body } => (body.OpenBraceToken, body.CloseBraceToken),
      LocalFunctionStatementSyntax { Body: { } body } => (body.OpenBraceToken, body.CloseBraceToken),
      AccessorDeclarationSyntax { Body: { } body } => (body.OpenBraceToken, body.CloseBraceToken),
      AccessorListSyntax accessors => (accessors.OpenBraceToken, accessors.CloseBraceToken),
      AnonymousFunctionExpressionSyntax { Block: { } body } => (body.OpenBraceToken, body.CloseBraceToken),
      SwitchStatementSyntax @switch => (@switch.OpenBraceToken, @switch.CloseBraceToken),
      InitializerExpressionSyntax initializer => (initializer.OpenBraceToken, initializer.CloseBraceToken),
      _ => (default, default),
    };
  }
}
