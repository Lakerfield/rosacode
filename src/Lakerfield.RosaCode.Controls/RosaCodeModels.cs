using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lakerfield.RosaCode;

// Action
public class ActionRequest
{
  public string Code { get; set; } = string.Empty;
  public int Line { get; set; }
  public int Column { get; set; }
  public int EndLine { get; set; }
  public int EndColumn { get; set; }
}

public class ActionResponse
{
  public IReadOnlyList<ActionAction> Actions { get; set; } = new List<ActionAction>();
}





// Code Execution
public class CodeExecutionRequest
{
  public string Code { get; set; } = string.Empty;
}

public class CodeExecutionResponse
{
  public string Output { get; set; } = string.Empty;
  public string Error { get; set; } = string.Empty;
}



// Code Completion
public class CompletionRequest
{
  public string Code { get; set; } = string.Empty;
  public int Line { get; set; }
  public int Column { get; set; }
}

public class CompletionResolveRequest
{
  public string Code { get; set; } = string.Empty;
  public int Line { get; set; }
  public int Column { get; set; }
  public int Id { get; set; }
}

public class CompletionResolveResponse
{
  public string Documentation { get; set; } = string.Empty;
}

// Cancel a request that is still running; Id is the id of that request
public class CancelRequest
{
  public int Id { get; set; }
}

public class CompletionResponse
{
  public List<CompletionItem> Suggestions { get; set; } = new();
}

public class CompletionItem
{
  public string Label { get; set; } = string.Empty;
  public string InsertText { get; set; } = string.Empty;
  public string Documentation { get; set; } = string.Empty;
}



// Format
public class FormatRequest
{
  public string Code { get; set; } = string.Empty;
  public int TabSize { get; set; }
  public bool InsertSpaces { get; set; }
}

public class FormatRangeRequest
{
  public string Code { get; set; } = string.Empty;
  public int StartLine { get; set; }
  public int StartColumn { get; set; }
  public int EndLine { get; set; }
  public int EndColumn { get; set; }
  public int TabSize { get; set; }
  public bool InsertSpaces { get; set; }
}

public class FormatRangeResponse
{
  public IReadOnlyList<ActionEdit> Edits { get; set; } = new List<ActionEdit>();
}

public class FormatResponse
{
  public string Format { get; set; } = string.Empty;
}



// Hover (Tooltip)
public class HoverRequest
{
  public string Code { get; set; } = string.Empty;
  public int Line { get; set; }
  public int Column { get; set; }
}

public class HoverResponse
{
  public string Tooltip { get; set; } = string.Empty;
}



// Diagnostics (Error Checking)
public class DiagnosticsRequest
{
  public string Code { get; set; } = string.Empty;
}

public class DiagnosticsResponse
{
  public List<DiagnosticItem> Errors { get; set; } = new();
}





// Signature Help
public class SignatureHelpRequest
{
  public string Code { get; set; } = string.Empty;
  public int Line { get; set; }
  public int Column { get; set; }
}

public class SignatureHelpResponse
{
  public List<SignatureItem> Signatures { get; set; } = new();
  public int ActiveSignature { get; set; }
  public int ActiveParameter { get; set; }
}







// Navigation, rename, outline, highlighting, hints, folding
public class RenameRequest
{
  public string Code { get; set; } = string.Empty;
  public int Line { get; set; }
  public int Column { get; set; }
  public string NewName { get; set; } = string.Empty;
}

public class RangeRequest
{
  public string Code { get; set; } = string.Empty;
  public int StartLine { get; set; }
  public int StartColumn { get; set; }
  public int EndLine { get; set; }
  public int EndColumn { get; set; }
}

public class LocationsResponse
{
  public IReadOnlyList<SymbolLocation> Locations { get; set; } = new List<SymbolLocation>();
}

public class SymbolsResponse
{
  public IReadOnlyList<DocumentSymbolItem> Symbols { get; set; } = new List<DocumentSymbolItem>();
}

public class InlayHintsResponse
{
  public IReadOnlyList<InlayHintItem> Hints { get; set; } = new List<InlayHintItem>();
}

public class FoldingResponse
{
  public IReadOnlyList<FoldingRangeItem> Ranges { get; set; } = new List<FoldingRangeItem>();
}
