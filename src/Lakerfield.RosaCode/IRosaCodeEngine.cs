
namespace Lakerfield.RosaCode
{
  public interface IRosaCodeEngine
  {
    /// <summary>The code the engine last received. The editor control itself returns its own current text.</summary>
    Task<string> GetCode();

    /// <summary>Code fixes and refactorings for the given range (start and end are equal for a plain cursor).</summary>
    Task<IReadOnlyList<ActionAction>> GetActions(string code, int line, int column, int endLine, int endColumn);

    Task<IEnumerable<Completion>> GetCompletions(string code, int line, int column);

    /// <summary>Documentation (markdown) of an item of the last <see cref="GetCompletions"/> result, by <see cref="Completion.Id"/>.</summary>
    Task<string> GetCompletionDescription(string code, int line, int column, int completionId);

    Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces);

    /// <summary>The changes that format the given range (the selection), for the formatter of the editor.</summary>
    Task<IReadOnlyList<ActionEdit>> GetFormattedRange(string code, int startLine, int startColumn, int endLine, int endColumn, int tabSize, bool insertSpaces);

    /// <summary>Hover text as markdown, or an empty string when there is nothing to show.</summary>
    Task<string> GetTooltip(string code, int line, int column);

    Task<IEnumerable<ActionDiagnostic>> GetDiagnostics(string code);

    Task<(IEnumerable<SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignatures(string code, int line, int column);

    /// <summary>Where the symbol at the position is declared in the code (nothing for symbols from a library).</summary>
    Task<IReadOnlyList<SymbolLocation>> GetDefinition(string code, int line, int column);

    /// <summary>All uses of the symbol at the position, including its declaration.</summary>
    Task<IReadOnlyList<SymbolLocation>> GetReferences(string code, int line, int column);

    Task<RenameInfo> GetRenameInfo(string code, int line, int column);

    /// <summary>The edits that rename the symbol at the position. <see cref="RenameResult.Error"/> is filled when that is not possible.</summary>
    Task<RenameResult> GetRenameEdits(string code, int line, int column, string newName);

    Task<IReadOnlyList<DocumentSymbolItem>> GetDocumentSymbols(string code);

    Task<SemanticTokensResult> GetSemanticTokens(string code);

    Task<IReadOnlyList<InlayHintItem>> GetInlayHints(string code, int startLine, int startColumn, int endLine, int endColumn);

    Task<IReadOnlyList<FoldingRangeItem>> GetFoldingRanges(string code);
  }
}
