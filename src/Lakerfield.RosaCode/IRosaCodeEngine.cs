
namespace Lakerfield.RosaCode
{
  public interface IRosaCodeEngine
  {
    Task<string> GetCode();
    Task<IReadOnlyList<ActionAction>> GetActions(string code, int line, int column, IReadOnlyList<ActionDiagnostic> diagnostics);
    Task<IEnumerable<Completion>> GetCompletions(string code, int line, int column);
    Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces);
    Task<string> GetTooltip(string code, int line, int column);
    Task<IEnumerable<ActionDiagnostic>> GetDiagnostics(string code);
    Task<(IEnumerable<SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignatures(string code, int line, int column);
  }
}
