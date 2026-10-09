using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lakerfield.RosaCode;


public class ActionAction
{
  public string Title { get; set; }
  public string Kind { get; set; }
  public List<ActionDiagnostic> Diagnostics { get; set; }
  public bool IsPreferred { get; set; }
  public ActionEdits Edit { get; set; }
}

public class ActionDiagnostic
{
  public string Message { get; set; }
  public int StartLineNumber { get; set; }
  public int StartColumn { get; set; }
  public int EndLineNumber { get; set; }
  public int EndColumn { get; set; }
  public string Severity { get; set; }
}
public class ActionEdits
{
  public List<ActionEdit> Edits { get; set; }
}

public class ActionEdit
{
  public ActionRange Range { get; set; }
  public string Text { get; set; }
}

public class ActionRange
{
  public int StartLineNumber { get; set; }
  public int StartColumn { get; set; }
  public int EndLineNumber { get; set; }
  public int EndColumn { get; set; }
}






public class Completion
{
  public string Label { get; set; }
  public string Kind { get; set; }
  public string InsertText { get; set; }
  public string[] Tags { get; set; }
  public string Documentation { get; set; }
}





public class DiagnosticItem
{
  public string Severity { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public int StartLineNumber { get; set; }
  public int StartColumn { get; set; }
  public int EndLineNumber { get; set; }
  public int EndColumn { get; set; }
}






public class SignatureItem
{
  public string Label { get; set; } = string.Empty;
  public MarkdownValue Documentation { get; set; } = new MarkdownValue("");
  public List<SignatureItemParameter> Parameters { get; set; } = new();
}

public class MarkdownValue
{
  public MarkdownValue(string value)
  {
    Value = value;
  }

  public string Value { get; set; }
  public bool SupportMarkdown { get; set; } = true;
}

public class SignatureItemParameter
{
  public string Label { get; set; } = string.Empty;
  public string Documentation { get; set; } = string.Empty;
}

public class SignatureInformation
{
  public string Label { get; set; }
  public string Documentation { get; set; }
  public List<ParameterInformation> Parameters { get; set; }
}

public class ParameterInformation
{
  public string Label { get; set; }
  public string Documentation { get; set; }
}


