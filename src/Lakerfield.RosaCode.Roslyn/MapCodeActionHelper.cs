using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lakerfield.RosaCode
{
  internal class MapCodeActionHelper
  {
    public static ActionEdit? CreateTextEdit(TextChange change, string title, SyntaxNode syntaxRoot, SourceText text)
    {
      if (title.StartsWith("Add using") || (title.StartsWith("Implement interface") && change.NewText.Trim().StartsWith("using ")))
      {
        return new ActionEdit
        {
          Range = new ActionRange { StartLineNumber = 1, StartColumn = 1, EndLineNumber = 1, EndColumn = 1 },
          Text = change.NewText
        };
      }

      if (title.StartsWith("Implement interface"))
      {
        var classDecl = syntaxRoot.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(cd => cd.Span.Contains(change.Span.Start));

        if (classDecl != null)
        {
          var pos = text.Lines.GetLinePosition(classDecl.CloseBraceToken.Span.Start);
          var indent = "  ";
          var newText = classDecl.Members.Any()
              ? $"\r\n{indent}{change.NewText.TrimEnd()}\r\n"
              : $"{change.NewText.TrimEnd()}\r\n";

          return new ActionEdit
          {
            Range = new ActionRange { StartLineNumber = pos.Line + 1, StartColumn = pos.Character + 1, EndLineNumber = pos.Line + 1, EndColumn = pos.Character + 1 },
            Text = newText
          };
        }
      }

      return null; // Or handle default case as needed
    }
  }
}
