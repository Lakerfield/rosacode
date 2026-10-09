using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine
  {
    /// <summary>
    /// Formats the document with the Roslyn formatter and the indentation of the editor.
    /// Only whitespace is changed: the line breaks and blank lines of the author stay as they are.
    /// </summary>
    public Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var document = UpdateCode(code);
        var root = await document.GetSyntaxRootAsync(ct);
        if (root == null)
          return code;

        var formattedRoot = Formatter.Format(root, _workspace, CreateFormattingOptions(code, tabSize, insertSpaces), ct);
        return formattedRoot.ToFullString();
      }, cancellationToken);

    /// <summary>The changes that format the selection, for "Format Selection" of the editor.</summary>
    public Task<IReadOnlyList<ActionEdit>> GetFormattedRange(string code, int startLine, int startColumn, int endLine, int endColumn, int tabSize, bool insertSpaces, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var document = UpdateCode(code);
        var text = await document.GetTextAsync(ct);
        var root = await document.GetSyntaxRootAsync(ct);
        if (root == null)
          return (IReadOnlyList<ActionEdit>)Array.Empty<ActionEdit>();

        var start = GetPosition(text, startLine, startColumn);
        var end = GetPosition(text, endLine, endColumn);
        var span = TextSpan.FromBounds(Math.Min(start, end), Math.Max(start, end));

        var changes = Formatter.GetFormattedTextChanges(root, span, _workspace, CreateFormattingOptions(code, tabSize, insertSpaces), ct);

        // changes that do not change anything are left out
        return changes
          .Where(change => text.ToString(change.Span) != change.NewText)
          .Select(change => CreateEdit(text, change))
          .ToList();
      }, cancellationToken);

    private OptionSet CreateFormattingOptions(string code, int tabSize, bool insertSpaces)
    {
      // new line breaks (from braces and so on) follow the line breaks the code already has
      var newLine = code.Contains("\r\n") || !code.Contains('\n') ? "\r\n" : "\n";

      return _workspace.Options
        .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, !insertSpaces)
        .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, tabSize)
        .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, tabSize)
        .WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, newLine)
        .WithChangedOption(FormattingOptions.SmartIndent, LanguageNames.CSharp, FormattingOptions.IndentStyle.Smart)
        .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, true)
        .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, true)
        .WithChangedOption(CSharpFormattingOptions.NewLineForMembersInObjectInit, true)
        .WithChangedOption(CSharpFormattingOptions.SpacingAfterMethodDeclarationName, true)
        .WithChangedOption(CSharpFormattingOptions.SpaceWithinMethodCallParentheses, false)
        .WithChangedOption(CSharpFormattingOptions.IndentBlock, true)
        .WithChangedOption(CSharpFormattingOptions.IndentBraces, false);
    }
  }
}
