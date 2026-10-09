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

  /// <summary>Diagnostic id, for example CS0103.</summary>
  public string Id { get; set; } = string.Empty;
  public string HelpLink { get; set; } = string.Empty;
  /// <summary>Roslyn custom tags, for example "Unnecessary" (unused code).</summary>
  public string[] Tags { get; set; } = Array.Empty<string>();
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

  /// <summary>Position in the last completion list, to ask for the documentation later.</summary>
  public int Id { get; set; }
  /// <summary>Shown right of the label, for example the namespace.</summary>
  public string InlineDescription { get; set; } = string.Empty;
  public string FilterText { get; set; } = string.Empty;
  public string SortText { get; set; } = string.Empty;
  public bool Preselect { get; set; }

  /// <summary>Characters that accept the item while typing (for example ".").</summary>
  public string CommitCharacters { get; set; } = string.Empty;
  /// <summary>The text to replace when it is not the word in front of the cursor, for items that insert more than their name (override, partial).</summary>
  public ActionRange? ReplaceRange { get; set; }
}





public class DiagnosticItem
{
  public string Severity { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public int StartLineNumber { get; set; }
  public int StartColumn { get; set; }
  public int EndLineNumber { get; set; }
  public int EndColumn { get; set; }
  public string Id { get; set; } = string.Empty;
  public string HelpLink { get; set; } = string.Empty;
  public string[] Tags { get; set; } = Array.Empty<string>();
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







// Navigation: go to definition, find references, highlights
public class SymbolLocation
{
  public ActionRange Range { get; set; } = new();
  /// <summary>The place where the symbol is declared, as opposed to a use of it.</summary>
  public bool IsDeclaration { get; set; }
  /// <summary>A use that changes the value (assignment, ref/out argument, ++).</summary>
  public bool IsWrite { get; set; }
}

// Rename
public class RenameInfo
{
  public bool CanRename { get; set; }
  /// <summary>The name that can be renamed, with its range.</summary>
  public ActionRange? Range { get; set; }
  public string Text { get; set; } = string.Empty;
  /// <summary>Why this cannot be renamed.</summary>
  public string Error { get; set; } = string.Empty;
}

public class RenameResult
{
  public List<ActionEdit> Edits { get; set; } = new();
  public string Error { get; set; } = string.Empty;
}

// Outline of the document: types and their members. Flat, a symbol points at its parent (-1 for the top level).
public class DocumentSymbolItem
{
  public int Id { get; set; }
  public int ParentId { get; set; } = -1;
  public string Name { get; set; } = string.Empty;
  public string Detail { get; set; } = string.Empty;
  /// <summary>Namespace, Class, Struct, Interface, Enum, Method, Constructor, Property, Field, Constant, Event, EnumMember, Function, Operator</summary>
  public string Kind { get; set; } = string.Empty;
  /// <summary>The whole declaration.</summary>
  public ActionRange Range { get; set; } = new();
  /// <summary>The name.</summary>
  public ActionRange SelectionRange { get; set; } = new();
}

/// <summary>
/// Classified identifiers of the document: <see cref="Data"/> holds five numbers per token (relative line, relative start,
/// length, index in <see cref="TokenTypes"/>, bit set of indexes in <see cref="TokenModifiers"/>), as in the language server protocol.
/// </summary>
public class SemanticTokensResult
{
  public string[] TokenTypes { get; set; } = Array.Empty<string>();
  public string[] TokenModifiers { get; set; } = Array.Empty<string>();
  public int[] Data { get; set; } = Array.Empty<int>();
}

/// <summary>A hint in the text: the name of a parameter in front of an argument, or the type of a "var".</summary>
public class InlayHintItem
{
  public int Line { get; set; }
  public int Column { get; set; }
  public string Label { get; set; } = string.Empty;
  /// <summary>Type or Parameter</summary>
  public string Kind { get; set; } = string.Empty;
  public bool PaddingLeft { get; set; }
  public bool PaddingRight { get; set; }
}

public class FoldingRangeItem
{
  /// <summary>The line that stays visible.</summary>
  public int StartLine { get; set; }
  /// <summary>The last line that is hidden.</summary>
  public int EndLine { get; set; }
  /// <summary>Region, Comment, Imports, or empty.</summary>
  public string Kind { get; set; } = string.Empty;
}
