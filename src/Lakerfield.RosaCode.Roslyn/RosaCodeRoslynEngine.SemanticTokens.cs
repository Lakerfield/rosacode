using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine
  {
    // The token types and modifiers of the language server protocol that the editor knows.
    private static readonly string[] _semanticTokenTypes =
    {
      "namespace", "class", "enum", "interface", "struct", "typeParameter", "parameter", "variable", "property", "event", "enumMember", "method",
    };

    private static readonly string[] _semanticTokenModifiers = { "static", "readonly" };

    // What Roslyn calls a classification -> token type, and the modifier that goes with it. Keywords, strings and
    // comments are not here: the grammar of the editor colours those already; this is about what a name stands for.
    private static readonly Dictionary<string, (string type, string? modifier)> _semanticClassifications = new()
    {
      [ClassificationTypeNames.NamespaceName] = ("namespace", null),
      [ClassificationTypeNames.ClassName] = ("class", null),
      [ClassificationTypeNames.RecordClassName] = ("class", null),
      [ClassificationTypeNames.DelegateName] = ("class", null),
      [ClassificationTypeNames.ModuleName] = ("class", null),
      [ClassificationTypeNames.EnumName] = ("enum", null),
      [ClassificationTypeNames.InterfaceName] = ("interface", null),
      [ClassificationTypeNames.StructName] = ("struct", null),
      [ClassificationTypeNames.RecordStructName] = ("struct", null),
      [ClassificationTypeNames.TypeParameterName] = ("typeParameter", null),
      [ClassificationTypeNames.ParameterName] = ("parameter", null),
      [ClassificationTypeNames.LocalName] = ("variable", null),
      [ClassificationTypeNames.ConstantName] = ("variable", "readonly"),
      [ClassificationTypeNames.FieldName] = ("property", null),
      [ClassificationTypeNames.PropertyName] = ("property", null),
      [ClassificationTypeNames.EventName] = ("event", null),
      [ClassificationTypeNames.EnumMemberName] = ("enumMember", "readonly"),
      [ClassificationTypeNames.MethodName] = ("method", null),
      [ClassificationTypeNames.ExtensionMethodName] = ("method", null),
    };

    public Task<SemanticTokensResult> GetSemanticTokens(string code)
      => GetSemanticTokens(code, CancellationToken.None);

    public Task<SemanticTokensResult> GetSemanticTokens(string code, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var document = UpdateCode(code);
        var text = await document.GetTextAsync(ct);

        var spans = (await Classifier.GetClassifiedSpansAsync(document, new TextSpan(0, text.Length), ct)).ToList();

        // "static symbol" is a span on top of the span of the name
        var staticSpans = new HashSet<TextSpan>(spans.Where(s => s.ClassificationType == ClassificationTypeNames.StaticSymbol).Select(s => s.TextSpan));

        var data = new List<int>();
        var previousLine = 0;
        var previousStart = 0;

        foreach (var span in spans
          .Where(s => _semanticClassifications.ContainsKey(s.ClassificationType))
          .GroupBy(s => s.TextSpan).Select(g => g.First())
          .OrderBy(s => s.TextSpan.Start))
        {
          var lines = text.Lines.GetLinePositionSpan(span.TextSpan);
          if (lines.Start.Line != lines.End.Line)
            continue; // a name does not span lines

          var (type, modifier) = _semanticClassifications[span.ClassificationType];

          var modifiers = 0;
          if (modifier != null)
            modifiers |= 1 << Array.IndexOf(_semanticTokenModifiers, modifier);
          if (staticSpans.Contains(span.TextSpan))
            modifiers |= 1 << Array.IndexOf(_semanticTokenModifiers, "static");

          var line = lines.Start.Line;
          var start = lines.Start.Character;

          data.Add(line - previousLine);
          data.Add(line == previousLine ? start - previousStart : start);
          data.Add(span.TextSpan.Length);
          data.Add(Array.IndexOf(_semanticTokenTypes, type));
          data.Add(modifiers);

          previousLine = line;
          previousStart = start;
        }

        return new SemanticTokensResult
        {
          TokenTypes = _semanticTokenTypes,
          TokenModifiers = _semanticTokenModifiers,
          Data = data.ToArray(),
        };
      }, cancellationToken);
  }
}
