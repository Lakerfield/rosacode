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
  public IReadOnlyList<ActionDiagnostic> Diagnostics { get; set; } = new List<ActionDiagnostic>();
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


