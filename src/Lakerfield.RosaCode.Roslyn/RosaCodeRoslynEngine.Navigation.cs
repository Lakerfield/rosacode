using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Lakerfield.RosaCode
{
  public partial class RosaCodeRoslynEngine
  {
    // What the editor points at: the identifier under the pointer and the symbol it stands for (a use of it, or its declaration)
    private record SymbolContext(Document Document, SourceText Text, SemanticModel Model, SyntaxNode Root, SyntaxToken Token, ISymbol? Symbol);

    private async Task<SymbolContext?> GetSymbolContext(string code, int line, int column, CancellationToken cancellationToken)
    {
      var document = UpdateCode(code);
      var text = await document.GetTextAsync(cancellationToken);
      var root = await document.GetSyntaxRootAsync(cancellationToken);
      var model = await document.GetSemanticModelAsync(cancellationToken);
      if (root == null || model == null)
        return null;

      var position = GetPosition(text, line, column);
      var token = FindToken(root, position);
      if (token.Parent == null || token.Span.IsEmpty)
        return null;

      var info = model.GetSymbolInfo(token.Parent, cancellationToken);
      var symbol = info.Symbol
        ?? info.CandidateSymbols.FirstOrDefault()
        ?? model.GetDeclaredSymbol(token.Parent, cancellationToken);

      return new SymbolContext(document, text, model, root, token, symbol);
    }

    // A pointer at the end of a name still belongs to that name
    private static SyntaxToken FindToken(SyntaxNode root, int position)
    {
      var token = root.FindToken(position);
      if (!token.IsKind(SyntaxKind.IdentifierToken) && position > 0)
      {
        var previous = root.FindToken(position - 1);
        if (previous.IsKind(SyntaxKind.IdentifierToken) && previous.Span.End >= position)
          token = previous;
      }

      return token;
    }

    // The symbol as it is declared: not a generic instantiation, not an extension method called as a member
    private static ISymbol GetDefinitionSymbol(ISymbol symbol)
      => symbol is IMethodSymbol method ? (method.ReducedFrom ?? method).OriginalDefinition : symbol.OriginalDefinition;

    // The places in this code where the symbol is declared
    private static List<Location> GetSourceLocations(ISymbol symbol, SyntaxTree tree)
    {
      var locations = symbol.Locations.Where(l => l.IsInSource && l.SourceTree == tree).ToList();

      // a constructor that nobody wrote: the class has it
      if (locations.Count == 0 && symbol is IMethodSymbol { MethodKind: MethodKind.Constructor, IsImplicitlyDeclared: true } constructor)
        locations = constructor.ContainingType.Locations.Where(l => l.IsInSource && l.SourceTree == tree).ToList();

      return locations;
    }



    public Task<IReadOnlyList<SymbolLocation>> GetDefinition(string code, int line, int column)
      => GetDefinition(code, line, column, CancellationToken.None);

    public Task<IReadOnlyList<SymbolLocation>> GetDefinition(string code, int line, int column, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var context = await GetSymbolContext(code, line, column, ct);
        if (context?.Symbol == null)
          return (IReadOnlyList<SymbolLocation>)Array.Empty<SymbolLocation>();

        return GetSourceLocations(GetDefinitionSymbol(context.Symbol), context.Root.SyntaxTree)
          .Select(location => new SymbolLocation { Range = CreateRange(context.Text, location.SourceSpan), IsDeclaration = true })
          .ToList();
      }, cancellationToken);



    public Task<IReadOnlyList<SymbolLocation>> GetReferences(string code, int line, int column)
      => GetReferences(code, line, column, CancellationToken.None);

    public Task<IReadOnlyList<SymbolLocation>> GetReferences(string code, int line, int column, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var context = await GetSymbolContext(code, line, column, ct);
        if (context?.Symbol == null)
          return (IReadOnlyList<SymbolLocation>)Array.Empty<SymbolLocation>();

        var referenced = await SymbolFinder.FindReferencesAsync(GetDefinitionSymbol(context.Symbol), context.Document.Project.Solution, ct);

        var results = new Dictionary<TextSpan, SymbolLocation>();
        foreach (var symbol in referenced)
        {
          foreach (var location in GetSourceLocations(symbol.Definition, context.Root.SyntaxTree))
            results[location.SourceSpan] = new SymbolLocation { Range = CreateRange(context.Text, location.SourceSpan), IsDeclaration = true };

          foreach (var reference in symbol.Locations.Where(r => r.Document.Id == context.Document.Id))
          {
            var span = reference.Location.SourceSpan;
            if (results.ContainsKey(span))
              continue;

            var node = context.Root.FindToken(span.Start).Parent;
            results[span] = new SymbolLocation { Range = CreateRange(context.Text, span), IsWrite = node != null && IsWrittenTo(node) };
          }
        }

        return results.OrderBy(r => r.Key.Start).Select(r => r.Value).ToList();
      }, cancellationToken);

    // Is this use of a variable one that changes it: an assignment, ++, or a ref/out argument
    private static bool IsWrittenTo(SyntaxNode name)
    {
      var expression = name;
      if (expression.Parent is MemberAccessExpressionSyntax access && access.Name == expression)
        expression = access;

      return expression.Parent switch
      {
        AssignmentExpressionSyntax assignment => assignment.Left == expression,
        PrefixUnaryExpressionSyntax prefix => prefix.IsKind(SyntaxKind.PreIncrementExpression) || prefix.IsKind(SyntaxKind.PreDecrementExpression),
        PostfixUnaryExpressionSyntax postfix => postfix.IsKind(SyntaxKind.PostIncrementExpression) || postfix.IsKind(SyntaxKind.PostDecrementExpression),
        ArgumentSyntax argument => argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword) || argument.RefKindKeyword.IsKind(SyntaxKind.RefKeyword),
        _ => false,
      };
    }



    public Task<RenameInfo> GetRenameInfo(string code, int line, int column)
      => GetRenameInfo(code, line, column, CancellationToken.None);

    public Task<RenameInfo> GetRenameInfo(string code, int line, int column, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var context = await GetSymbolContext(code, line, column, ct);
        var error = GetRenameError(context);
        if (error != null)
          return new RenameInfo { CanRename = false, Error = error };

        return new RenameInfo
        {
          CanRename = true,
          Range = CreateRange(context!.Text, context.Token.Span),
          Text = context.Token.ValueText,
        };
      }, cancellationToken);

    // Only names that are declared in this code can be renamed
    private static string? GetRenameError(SymbolContext? context)
    {
      const string cannot = "You cannot rename this element.";

      if (context?.Symbol == null || !context.Token.IsKind(SyntaxKind.IdentifierToken))
        return cannot;

      // "var" stands for a type, but it is not its name
      if (context.Token.Parent is IdentifierNameSyntax { IsVar: true } && context.Symbol is not ILocalSymbol)
        return cannot;

      var definition = GetDefinitionSymbol(context.Symbol);
      if (definition.IsImplicitlyDeclared && definition is not IMethodSymbol { MethodKind: MethodKind.Constructor })
        return cannot;

      return definition.Locations.Any(l => l.IsInSource) ? null : "You cannot rename an element that is defined in a library.";
    }

    public Task<RenameResult> GetRenameEdits(string code, int line, int column, string newName)
      => GetRenameEdits(code, line, column, newName, CancellationToken.None);

    public Task<RenameResult> GetRenameEdits(string code, int line, int column, string newName, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var context = await GetSymbolContext(code, line, column, ct);
        var error = GetRenameError(context);
        if (error != null)
          return new RenameResult { Error = error };

        if (!SyntaxFacts.IsValidIdentifier(newName))
          return new RenameResult { Error = $"'{newName}' is not a valid name." };

        var document = context!.Document;
        var renamed = await Renamer.RenameSymbolAsync(document.Project.Solution, GetDefinitionSymbol(context.Symbol!), new SymbolRenameOptions(), newName, ct);

        var changedDocument = renamed.GetDocument(document.Id);
        if (changedDocument == null)
          return new RenameResult { Error = "Nothing was renamed." };

        var result = new RenameResult();
        foreach (var change in await changedDocument.GetTextChangesAsync(document, ct))
          result.Edits.Add(CreateEdit(context.Text, change));

        return result;
      }, cancellationToken);



    public Task<IReadOnlyList<DocumentSymbolItem>> GetDocumentSymbols(string code)
      => GetDocumentSymbols(code, CancellationToken.None);

    // The outline: namespaces, types and their members. Syntax only, so it also works while the code does not compile.
    public Task<IReadOnlyList<DocumentSymbolItem>> GetDocumentSymbols(string code, CancellationToken cancellationToken)
      => RunExclusive(async ct =>
      {
        var document = UpdateCode(code);
        var text = await document.GetTextAsync(ct);
        var root = await document.GetSyntaxRootAsync(ct);

        var symbols = new List<DocumentSymbolItem>();
        if (root != null)
          CollectSymbols(root, -1, text, symbols, ct);

        return (IReadOnlyList<DocumentSymbolItem>)symbols;
      }, cancellationToken);

    private static void CollectSymbols(SyntaxNode container, int parentId, SourceText text, List<DocumentSymbolItem> symbols, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      foreach (var node in container.ChildNodes())
      {
        switch (node)
        {
          case BaseNamespaceDeclarationSyntax @namespace:
            CollectSymbols(@namespace, Add(symbols, parentId, text, node, "Namespace", @namespace.Name.ToString(), string.Empty, @namespace.Name.Span), text, symbols, cancellationToken);
            break;

          case BaseTypeDeclarationSyntax type:
            var kind = type.Kind() switch
            {
              SyntaxKind.StructDeclaration or SyntaxKind.RecordStructDeclaration => "Struct",
              SyntaxKind.InterfaceDeclaration => "Interface",
              SyntaxKind.EnumDeclaration => "Enum",
              _ => "Class",
            };
            var typeName = type.Identifier.Text + (type is TypeDeclarationSyntax { TypeParameterList: { } typeParameters } ? typeParameters.ToString() : string.Empty);
            CollectSymbols(type, Add(symbols, parentId, text, node, kind, typeName, string.Empty, type.Identifier.Span), text, symbols, cancellationToken);
            break;

          case DelegateDeclarationSyntax @delegate:
            Add(symbols, parentId, text, node, "Function", @delegate.Identifier.Text, $"{@delegate.ParameterList} : {@delegate.ReturnType}", @delegate.Identifier.Span);
            break;

          case MethodDeclarationSyntax method:
            Add(symbols, parentId, text, node, "Method", method.Identifier.Text + (method.TypeParameterList?.ToString() ?? string.Empty), $"{method.ParameterList} : {method.ReturnType}", method.Identifier.Span);
            break;

          case ConstructorDeclarationSyntax constructor:
            Add(symbols, parentId, text, node, "Constructor", constructor.Identifier.Text, constructor.ParameterList.ToString(), constructor.Identifier.Span);
            break;

          case DestructorDeclarationSyntax destructor:
            Add(symbols, parentId, text, node, "Method", "~" + destructor.Identifier.Text, "()", destructor.Identifier.Span);
            break;

          case PropertyDeclarationSyntax property:
            Add(symbols, parentId, text, node, "Property", property.Identifier.Text, property.Type.ToString(), property.Identifier.Span);
            break;

          case IndexerDeclarationSyntax indexer:
            Add(symbols, parentId, text, node, "Property", "this[]", $"{indexer.ParameterList} : {indexer.Type}", indexer.ThisKeyword.Span);
            break;

          case FieldDeclarationSyntax field:
            var isConstant = field.Modifiers.Any(SyntaxKind.ConstKeyword);
            foreach (var variable in field.Declaration.Variables)
              Add(symbols, parentId, text, variable, isConstant ? "Constant" : "Field", variable.Identifier.Text, field.Declaration.Type.ToString(), variable.Identifier.Span);
            break;

          case EventDeclarationSyntax @event:
            Add(symbols, parentId, text, node, "Event", @event.Identifier.Text, @event.Type.ToString(), @event.Identifier.Span);
            break;

          case EventFieldDeclarationSyntax eventField:
            foreach (var variable in eventField.Declaration.Variables)
              Add(symbols, parentId, text, variable, "Event", variable.Identifier.Text, eventField.Declaration.Type.ToString(), variable.Identifier.Span);
            break;

          case OperatorDeclarationSyntax @operator:
            Add(symbols, parentId, text, node, "Operator", "operator " + @operator.OperatorToken.Text, $"{@operator.ParameterList} : {@operator.ReturnType}", @operator.OperatorToken.Span);
            break;

          case ConversionOperatorDeclarationSyntax conversion:
            Add(symbols, parentId, text, node, "Operator", $"{conversion.ImplicitOrExplicitKeyword.Text} operator {conversion.Type}", conversion.ParameterList.ToString(), conversion.Type.Span);
            break;

          case EnumMemberDeclarationSyntax enumMember:
            Add(symbols, parentId, text, node, "EnumMember", enumMember.Identifier.Text, string.Empty, enumMember.Identifier.Span);
            break;
        }
      }
    }

    private static int Add(List<DocumentSymbolItem> symbols, int parentId, SourceText text, SyntaxNode node, string kind, string name, string detail, TextSpan selection)
    {
      var id = symbols.Count;
      symbols.Add(new DocumentSymbolItem
      {
        Id = id,
        ParentId = parentId,
        Name = name,
        Detail = detail,
        Kind = kind,
        Range = CreateRange(text, node.Span),
        SelectionRange = CreateRange(text, selection),
      });
      return id;
    }
  }
}
