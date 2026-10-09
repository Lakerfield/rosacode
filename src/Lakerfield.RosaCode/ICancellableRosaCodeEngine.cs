
namespace Lakerfield.RosaCode
{
  /// <summary>
  /// An engine that can stop work for requests the editor no longer needs (typing continues, the pointer moves on).
  /// Kept apart from <see cref="IRosaCodeEngine"/> because a remote (RPC) engine cannot pass a token over the wire.
  /// The editor uses these overloads when the engine implements this interface.
  /// </summary>
  public interface ICancellableRosaCodeEngine : IRosaCodeEngine
  {
    Task<IReadOnlyList<ActionAction>> GetActions(string code, int line, int column, int endLine, int endColumn, CancellationToken cancellationToken);
    Task<IEnumerable<Completion>> GetCompletions(string code, int line, int column, CancellationToken cancellationToken);
    Task<string> GetCompletionDescription(string code, int line, int column, int completionId, CancellationToken cancellationToken);
    Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces, CancellationToken cancellationToken);
    Task<IReadOnlyList<ActionEdit>> GetFormattedRange(string code, int startLine, int startColumn, int endLine, int endColumn, int tabSize, bool insertSpaces, CancellationToken cancellationToken);
    Task<string> GetTooltip(string code, int line, int column, CancellationToken cancellationToken);
    Task<IEnumerable<ActionDiagnostic>> GetDiagnostics(string code, CancellationToken cancellationToken);
    Task<(IEnumerable<SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignatures(string code, int line, int column, CancellationToken cancellationToken);
    Task<IReadOnlyList<SymbolLocation>> GetDefinition(string code, int line, int column, CancellationToken cancellationToken);
    Task<IReadOnlyList<SymbolLocation>> GetReferences(string code, int line, int column, CancellationToken cancellationToken);
    Task<RenameInfo> GetRenameInfo(string code, int line, int column, CancellationToken cancellationToken);
    Task<RenameResult> GetRenameEdits(string code, int line, int column, string newName, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentSymbolItem>> GetDocumentSymbols(string code, CancellationToken cancellationToken);
    Task<SemanticTokensResult> GetSemanticTokens(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<InlayHintItem>> GetInlayHints(string code, int startLine, int startColumn, int endLine, int endColumn, CancellationToken cancellationToken);
    Task<IReadOnlyList<FoldingRangeItem>> GetFoldingRanges(string code, CancellationToken cancellationToken);
  }
}
