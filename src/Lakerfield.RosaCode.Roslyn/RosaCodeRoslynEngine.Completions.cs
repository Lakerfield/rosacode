using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using System.Text;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine
  {
    // The items of the last completion list: the editor asks for the documentation of one item at a time
    private IReadOnlyList<CompletionItem> _completionItems = Array.Empty<CompletionItem>();

    // Typing one of these accepts the selected item. Roslyn has many more (space, operators, ...), but the editor
    // always has an item selected, so accepting on those would change what the user is typing.
    private const string SafeCommitCharacters = ".(";

    // Items that insert more than their name (override, partial method) need the change that Roslyn computes: limit the work
    private const int MaxComplexItems = 50;

    public Task<IEnumerable<Completion>> GetCompletions(string code, int line, int column, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var document = UpdateCode(code);

        var completionService = CompletionService.GetService(document);
        if (completionService == null)
          return Enumerable.Empty<Completion>();

        var text = await document.GetTextAsync(ct);
        var position = GetPosition(text, line, column);

        var completionList = await completionService.GetCompletionsAsync(document, position, cancellationToken: ct);
        _completionItems = completionList?.ItemsList ?? (IReadOnlyList<CompletionItem>)Array.Empty<CompletionItem>();

        // Roslyn is in "suggestion mode" where the user names something new (a variable): nothing is accepted by typing then
        var suggestionMode = completionList?.SuggestionModeItem != null;
        var defaultCommitCharacters = completionService.GetRules().DefaultCommitCharacters;

        var complexItems = 0;
        var completions = new List<Completion>(_completionItems.Count);
        for (var index = 0; index < _completionItems.Count; index++)
        {
          var item = _completionItems[index];

          var completion = new Completion
          {
            Id = index,
            Label = item.DisplayText,
            Kind = item.Tags.FirstOrDefault() ?? "Text",
            InsertText = item.DisplayText,
            Tags = item.Tags.ToArray(),
            InlineDescription = item.InlineDescription ?? string.Empty,
            FilterText = item.FilterText,
            SortText = item.SortText,
            Preselect = item.Rules.MatchPriority == MatchPriority.Preselect,
            // not for items that insert a whole declaration: a typed "(" must not paste a method first
            CommitCharacters = suggestionMode || item.IsComplexTextEdit ? string.Empty : GetCommitCharacters(item, defaultCommitCharacters),
          };

          if (item.IsComplexTextEdit && complexItems++ < MaxComplexItems)
          {
            var change = await completionService.GetChangeAsync(document, item, cancellationToken: ct);
            completion.InsertText = change.TextChange.NewText ?? string.Empty;
            completion.ReplaceRange = CreateRange(text, change.TextChange.Span);
          }

          completions.Add(completion);
        }

        return (IEnumerable<Completion>)completions;
      }, cancellationToken);

    // The commit characters of the item: the defaults of Roslyn, changed by the rules of the item
    private static string GetCommitCharacters(CompletionItem item, IEnumerable<char> defaultCommitCharacters)
    {
      var characters = new HashSet<char>(defaultCommitCharacters);

      foreach (var rule in item.Rules.CommitCharacterRules)
      {
        switch (rule.Kind)
        {
          case CharacterSetModificationKind.Add:
            characters.UnionWith(rule.Characters);
            break;
          case CharacterSetModificationKind.Remove:
            characters.ExceptWith(rule.Characters);
            break;
          case CharacterSetModificationKind.Replace:
            characters = new HashSet<char>(rule.Characters);
            break;
        }
      }

      return new string(SafeCommitCharacters.Where(characters.Contains).ToArray());
    }

    public Task<string> GetCompletionDescription(string code, int line, int column, int completionId, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        if (completionId < 0 || completionId >= _completionItems.Count)
          return string.Empty;

        var document = UpdateCode(code);

        var completionService = CompletionService.GetService(document);
        if (completionService == null)
          return string.Empty;

        var description = await completionService.GetDescriptionAsync(document, _completionItems[completionId], ct);
        return description == null ? string.Empty : DescriptionToMarkdown(description);
      }, cancellationToken);

    // Roslyn gives the signature first, then the documentation
    private static string DescriptionToMarkdown(CompletionDescription description)
    {
      var text = description.Text.Replace("\r\n", "\n").Trim();
      if (text.Length == 0)
        return string.Empty;

      var lineBreak = text.IndexOf('\n');
      var signature = lineBreak < 0 ? text : text.Substring(0, lineBreak);
      var documentation = lineBreak < 0 ? string.Empty : text.Substring(lineBreak + 1).Trim();

      var result = new StringBuilder();
      result.AppendLine("```csharp").AppendLine(signature).Append("```");
      if (documentation.Length > 0)
        result.AppendLine().AppendLine().Append(documentation);

      return result.ToString();
    }
  }
}
