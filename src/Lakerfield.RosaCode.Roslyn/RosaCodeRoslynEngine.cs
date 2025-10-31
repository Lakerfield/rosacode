
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using System.Xml.Linq;

using static Basic.Reference.Assemblies.Net80.References;

namespace Lakerfield.RosaCode
{
  public class RosaCodeRoslynEngine : IRosaCodeEngine
  {
    private object _lock = new object();
    private AdhocWorkspace _workspace;
    private Document _document;


    public RosaCodeRoslynEngine(string name = "default", string code = "")
    {
      _workspace = new AdhocWorkspace();
      //var metadataReferences = GenerateMetadataReferences();

      var refs = new[] {
            MicrosoftCSharp,
            //MicrosoftVisualBasicCore,
            //MicrosoftVisualBasic,
            MicrosoftWin32Primitives,
            //MicrosoftWin32Registry,
            mscorlib,
            netstandard,
            SystemAppContext,
            SystemBuffers,
            SystemCollectionsConcurrent,
            SystemCollections,
            SystemCollectionsImmutable,
            SystemCollectionsNonGeneric,
            SystemCollectionsSpecialized,
            SystemComponentModelAnnotations,
            SystemComponentModelDataAnnotations,
            SystemComponentModel,
            SystemComponentModelEventBasedAsync,
            SystemComponentModelPrimitives,
            SystemComponentModelTypeConverter,
            SystemConfiguration,
            SystemConsole,
            SystemCore,
            SystemDataCommon,
            SystemDataDataSetExtensions,
            SystemData,
            SystemDiagnosticsContracts,
            SystemDiagnosticsDebug,
            SystemDiagnosticsDiagnosticSource,
            SystemDiagnosticsFileVersionInfo,
            SystemDiagnosticsProcess,
            SystemDiagnosticsStackTrace,
            SystemDiagnosticsTextWriterTraceListener,
            SystemDiagnosticsTools,
            SystemDiagnosticsTraceSource,
            SystemDiagnosticsTracing,
            Basic.Reference.Assemblies.Net80.References.System,
            //SystemDrawing,
            //SystemDrawingPrimitives,
            //SystemDynamicRuntime,
            //SystemFormatsAsn1,
            SystemGlobalizationCalendars,
            SystemGlobalization,
            SystemGlobalizationExtensions,
            SystemIOCompressionBrotli,
            SystemIOCompression,
            SystemIOCompressionFileSystem,
            SystemIOCompressionZipFile,
            SystemIO,
            SystemIOFileSystemAccessControl,
            SystemIOFileSystem,
            SystemIOFileSystemDriveInfo,
            SystemIOFileSystemPrimitives,
            SystemIOFileSystemWatcher,
            SystemIOIsolatedStorage,
            SystemIOMemoryMappedFiles,
            SystemIOPipesAccessControl,
            SystemIOPipes,
            SystemIOUnmanagedMemoryStream,
            SystemLinq,
            SystemLinqExpressions,
            SystemLinqParallel,
            SystemLinqQueryable,
            SystemMemory,
            SystemNet,
            SystemNetHttp,
            SystemNetHttpJson,
            SystemNetHttpListener,
            SystemNetMail,
            SystemNetNameResolution,
            SystemNetNetworkInformation,
            SystemNetPing,
            SystemNetPrimitives,
            SystemNetRequests,
            SystemNetSecurity,
            SystemNetServicePoint,
            SystemNetSockets,
            SystemNetWebClient,
            SystemNetWebHeaderCollection,
            SystemNetWebProxy,
            SystemNetWebSocketsClient,
            SystemNetWebSockets,
            SystemNumerics,
            SystemNumericsVectors,
            SystemObjectModel,
            SystemReflectionDispatchProxy,
            SystemReflection,
            SystemReflectionEmit,
            SystemReflectionEmitILGeneration,
            SystemReflectionEmitLightweight,
            SystemReflectionExtensions,
            SystemReflectionMetadata,
            SystemReflectionPrimitives,
            SystemReflectionTypeExtensions,
            SystemResourcesReader,
            SystemResourcesResourceManager,
            SystemResourcesWriter,
            SystemRuntimeCompilerServicesUnsafe,
            SystemRuntimeCompilerServicesVisualC,
            SystemRuntime,
            SystemRuntimeExtensions,
            SystemRuntimeHandles,
            SystemRuntimeInteropServices,
            SystemRuntimeInteropServicesRuntimeInformation,
            SystemRuntimeIntrinsics,
            SystemRuntimeLoader,
            SystemRuntimeNumerics,
            SystemRuntimeSerialization,
            SystemRuntimeSerializationFormatters,
            SystemRuntimeSerializationJson,
            SystemRuntimeSerializationPrimitives,
            SystemRuntimeSerializationXml,
            SystemSecurityAccessControl,
            SystemSecurityClaims,
            SystemSecurityCryptographyAlgorithms,
            SystemSecurityCryptographyCng,
            SystemSecurityCryptographyCsp,
            SystemSecurityCryptographyEncoding,
            SystemSecurityCryptographyOpenSsl,
            SystemSecurityCryptographyPrimitives,
            SystemSecurityCryptographyX509Certificates,
            SystemSecurity,
            SystemSecurityPrincipal,
            SystemSecurityPrincipalWindows,
            SystemSecuritySecureString,
            SystemServiceModelWeb,
            SystemServiceProcess,
            SystemTextEncodingCodePages,
            SystemTextEncoding,
            SystemTextEncodingExtensions,
            SystemTextEncodingsWeb,
            SystemTextJson,
            SystemTextRegularExpressions,
            SystemThreadingChannels,
            SystemThreading,
            SystemThreadingOverlapped,
            SystemThreadingTasksDataflow,
            SystemThreadingTasks,
            SystemThreadingTasksExtensions,
            SystemThreadingTasksParallel,
            SystemThreadingThread,
            SystemThreadingThreadPool,
            SystemThreadingTimer,
            SystemTransactions,
            SystemTransactionsLocal,
            SystemValueTuple,
        //SystemWeb,
        //SystemWebHttpUtility,
        //SystemWindows,
            SystemXml,
            SystemXmlLinq,
            SystemXmlReaderWriter,
            SystemXmlSerialization,
            SystemXmlXDocument,
            SystemXmlXmlDocument,
            SystemXmlXmlSerializer,
            SystemXmlXPath,
            SystemXmlXPathXDocument
            //WindowsBase,
          };

      var project = _workspace
      .CurrentSolution
      .AddProject($"{name} Project", name, LanguageNames.CSharp)
      .WithParseOptions(new CSharpParseOptions(documentationMode: DocumentationMode.Parse))
      .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
      //.AddMetadataReferences(refs)
      .AddMetadataReferences(XmlHelper.GetRefs(refs))
      .AddMetadataReferences(RosaCodeRoslynConstants.GetFilteredAppDomainAssemblyReferences().Select(assembly => MetadataReference.CreateFromFile(assembly.Location)))
      //.AddMetadataReferences(metadataReferences)
      ;

      _document = project.AddDocument($"{name}.cs", SourceText.From(code));

      _workspace.TryApplyChanges(_document.Project.Solution);
    }

    public Document UpdateCode(string code)
    {
      lock (_lock)
      {
        var newText = SourceText.From(code);

        var newSolution = _workspace.CurrentSolution.WithDocumentText(_document.Id, newText);

        _workspace.TryApplyChanges(newSolution);

        _document = _workspace.CurrentSolution.GetDocument(_document.Id);

        return _document;
      }
    }



    public async Task<string> GetCode()
    {
      var result = await _document.GetTextAsync();
      return result.ToString();
    }




    public async Task<IReadOnlyList<ActionAction>> GetActions(string code, int line, int column, IReadOnlyList<ActionDiagnostic> diagnostics)
    {
      var document = UpdateCode(code);
      var text = await document.GetTextAsync();
      var position = text.Lines[line - 1].Start + column - 1;
      var syntaxTree = await document.GetSyntaxTreeAsync();
      var semanticModel = await document.GetSemanticModelAsync();
      var token = syntaxTree.GetRoot().FindToken(position, findInsideTrivia: false);

      if (token.IsKind(SyntaxKind.None) || token.Span.IsEmpty || token.IsKind(SyntaxKind.EndOfFileToken))
      {
        token = token.GetPreviousToken();
      }

      var actions = new List<ActionAction>();
      var roslynDiagnostics = diagnostics?.Select(d =>
      {
        var severity = d.Severity == "Error" ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning;
        return Diagnostic.Create(
            "CS0103", // Default code; adjust if using Option 2 with Id
            "CS",
            d.Message,
            severity,
            severity, // defaultSeverity matches severity
            isSuppressed: false,
            warningLevel: severity == DiagnosticSeverity.Warning ? 1 : 0, // 1 for Warning, 0 for Error
            location: syntaxTree.GetLocation(
                TextSpan.FromBounds(
                    text.Lines[d.StartLineNumber - 1].Start + d.StartColumn - 1,
                    text.Lines[d.EndLineNumber - 1].Start + d.EndColumn - 1
                )
            ),
          isEnabledByDefault: true//checken, zelf toegevoegd
        );
      }).ToList() ?? new List<Diagnostic>();

      if (token.Parent != null)
      {
        var symbolName = token.Text;
        var compilation = semanticModel.Compilation;

        // Namespace suggestion logic
        var candidateNamespaces = new List<string>();
        foreach (var reference in compilation.References)
        {
          var assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
          if (assembly != null)
          {
            foreach (var type in assembly.GlobalNamespace.GetNamespaceMembers().SelectMany(ns => GetAllTypes(ns)))
            {
              if (type.Name == symbolName)
              {
                var namespaceName = type.ContainingNamespace.ToDisplayString();
                if (!candidateNamespaces.Contains(namespaceName) && !string.IsNullOrEmpty(namespaceName))
                {
                  candidateNamespaces.Add(namespaceName);
                }
              }
              else if (type.IsStatic && type.GetMembers(symbolName).Any())
              {
                var namespaceName = type.ContainingNamespace.ToDisplayString();
                if (!candidateNamespaces.Contains(namespaceName) && !string.IsNullOrEmpty(namespaceName))
                {
                  candidateNamespaces.Add(namespaceName);
                }
              }
              else if (type.GetMembers(symbolName).Any(m => m is IMethodSymbol method && method.IsExtensionMethod))
              {
                var namespaceName = type.ContainingNamespace.ToDisplayString();
                if (!candidateNamespaces.Contains(namespaceName) && !string.IsNullOrEmpty(namespaceName))
                {
                  candidateNamespaces.Add(namespaceName);
                }
              }
            }
          }
        }

        if (candidateNamespaces.Any())
        {
          var memberAccess = token.Parent.Ancestors().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
          TypeInfo? typeInfo = memberAccess != null ? semanticModel.GetTypeInfo(memberAccess.Expression) : null;
          var ienumerableT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
          var implementsIEnumerable = typeInfo.HasValue && typeInfo.Value.Type != null &&
                                      typeInfo.Value.Type.AllInterfaces.Any(i => i.OriginalDefinition.Equals(ienumerableT));

          foreach (var ns in candidateNamespaces)
          {
            bool shouldAddAction = false;

            var typeSymbol = compilation.GetTypeByMetadataName($"{ns}.{symbolName}");
            if (typeSymbol != null && (typeSymbol.TypeKind == TypeKind.Class || typeSymbol.TypeKind == TypeKind.Interface || typeSymbol.TypeKind == TypeKind.Struct))
            {
              shouldAddAction = true;
            }

            if (typeSymbol != null && typeSymbol.IsStatic && typeSymbol.GetMembers().Any(m => m.Name == "WriteLine"))
            {
              shouldAddAction = true;
            }

            var enumerableType = compilation.GetTypeByMetadataName($"{ns}.Enumerable");
            if (enumerableType != null && memberAccess != null)
            {
              var extensionMethods = enumerableType.GetMembers(symbolName)
                  .OfType<IMethodSymbol>()
                  .Where(m => m.IsExtensionMethod && m.Parameters.Length > 0 &&
                              (implementsIEnumerable || m.Parameters[0].Type.AllInterfaces.Any(i => i.OriginalDefinition.Equals(ienumerableT))));
              if (extensionMethods.Any())
              {
                shouldAddAction = true;
              }
            }

            if (shouldAddAction)
            {
              var relevantDiagnostics = roslynDiagnostics
                  .Where(d => d.Location.SourceSpan.Contains(position) || d.Id == "CS0103")
                  .Select(Convert)
                  .ToList();

              var codeAction = CodeAction.Create(
                  $"Add using {ns}",
                  async cancellationToken =>
                  {
                    var root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken);
                    var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns))
                              .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                    var updatedUsings = root.Usings.Add(newUsing)
                              .OrderBy(u => u.Name.ToString())
                              .ToArray();
                    var updatedRoot = root.WithUsings(SyntaxFactory.List(updatedUsings));
                    return document.WithSyntaxRoot(updatedRoot).Project.Solution;
                  },
                  $"AddUsing{ns}"
              );

              actions.Add(await MapCodeActionToDto(codeAction, relevantDiagnostics, document));
            }
          }
        }

        // Interface implementation logic
        var classDeclaration = token.Parent.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration != null)
        {
          var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
          var baseList = classDeclaration.BaseList?.Types;

          if (baseList != null)
          {
            foreach (var baseType in baseList)
            {
              var baseTypeSyntax = baseType.Type.ToString();
              INamedTypeSymbol interfaceSymbol = null;
              string requiredNamespace = null;

              if (classSymbol == null || classSymbol.Interfaces.All(i => i.Name != baseTypeSyntax))
              {
                foreach (var reference in compilation.References)
                {
                  var assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                  if (assembly != null)
                  {
                    interfaceSymbol = assembly.GlobalNamespace.GetNamespaceMembers()
                        .SelectMany(ns => GetAllTypes(ns))
                        .FirstOrDefault(t => t.Name == baseTypeSyntax && t.TypeKind == TypeKind.Interface) as INamedTypeSymbol;
                    if (interfaceSymbol != null)
                    {
                      requiredNamespace = interfaceSymbol.ContainingNamespace.ToDisplayString();
                      break;
                    }
                  }
                }
              }
              else
              {
                interfaceSymbol = classSymbol.Interfaces.FirstOrDefault(i => i.Name == baseTypeSyntax);
              }

              if (interfaceSymbol != null)
              {
                var allInterfaceMembers = interfaceSymbol.GetMembers();
                var unimplementedMembers = allInterfaceMembers
                    .Where(m => classSymbol == null || classSymbol.FindImplementationForInterfaceMember(m) == null)
                    .ToList();

                if (unimplementedMembers.Any() || roslynDiagnostics.Any(d => d.Id == "CS0535"))
                {
                  var relevantDiagnostics = roslynDiagnostics
                      .Where(d => d.Id == "CS0535" || d.Location.SourceSpan.Contains(position))
                      .Select(Convert)
                      .ToList();

                  var codeAction = CodeAction.Create(
                      $"Implement interface '{interfaceSymbol.Name}'",
                      async cancellationToken =>
                      {
                        var root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken);
                        var newClass = classDeclaration;

                        if (!string.IsNullOrEmpty(requiredNamespace) && !root.Usings.Any(u => u.Name.ToString() == requiredNamespace))
                        {
                          var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(requiredNamespace))
                                        .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                          root = root.WithUsings(SyntaxFactory.List(root.Usings.Add(newUsing).OrderBy(u => u.Name.ToString())));
                        }

                        foreach (var member in allInterfaceMembers)
                        {
                          if (member is IMethodSymbol method)
                          {
                            var methodDecl = SyntaxFactory.MethodDeclaration(
                                          SyntaxFactory.ParseTypeName(method.ReturnType.ToDisplayString()),
                                          method.Name)
                                          .WithParameterList(SyntaxFactory.ParameterList(
                                              SyntaxFactory.SeparatedList(method.Parameters.Select(p =>
                                                  SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                                      .WithType(SyntaxFactory.ParseTypeName(p.Type.ToDisplayString()))))))
                                          .WithBody(SyntaxFactory.Block(
                                              SyntaxFactory.ThrowStatement(
                                                  SyntaxFactory.ObjectCreationExpression(
                                                      SyntaxFactory.ParseTypeName("System.NotImplementedException"))
                                                  .WithArgumentList(SyntaxFactory.ArgumentList()))))
                                          .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
                            newClass = newClass.AddMembers(methodDecl);
                          }
                        }

                        return document.WithSyntaxRoot(root.ReplaceNode(classDeclaration, newClass)).Project.Solution;
                      },
                      $"ImplementInterface{interfaceSymbol.Name}"
                  );

                  actions.Add(await MapCodeActionToDto(codeAction, relevantDiagnostics, document));
                }
              }
            }
          }
        }
      }

      return actions;
    }

    private IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
    {
      foreach (var type in ns.GetTypeMembers())

      {
        yield return type;
      }
      foreach (var childNs in ns.GetNamespaceMembers())
      {
        foreach (var type in GetAllTypes(childNs))
        {
          yield return type;
        }
      }
    }

    private async Task<ActionAction> MapCodeActionToDto(CodeAction codeAction, List<ActionDiagnostic> diagnostics, Document document)
    {
      var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
      var textEdits = new List<ActionEdit>();

      foreach (var op in operations.OfType<ApplyChangesOperation>())
      {
        var changedDoc = op.ChangedSolution.GetDocument(document.Id);
        var changes = await changedDoc.GetTextChangesAsync(document);
        var syntaxRoot = await document.GetSyntaxRootAsync();
        var text = await document.GetTextAsync();

        textEdits.AddRange(changes
          .Select(change => MapCodeActionHelper.CreateTextEdit(change, codeAction.Title, syntaxRoot, text))
          .Where(x => x != null));
      }

      return new ActionAction
      {
        Title = codeAction.Title,
        Kind = "quickfix",
        Diagnostics = diagnostics,
        IsPreferred = true,
        Edit = new ActionEdits { Edits = textEdits }
      };
    }





    public async Task<IEnumerable<Completion>> GetCompletions(string code, int line, int column)
    {
      var document = UpdateCode(code);

      var completionService = CompletionService.GetService(document);

      var text = await document.GetTextAsync();
      var position = text.Lines[line - 1].Start + column - 1;

      var completionList = await completionService.GetCompletionsAsync(document, position);
      var items = completionList?.ItemsList.ToList();

      var syntaxTree = await document.GetSyntaxTreeAsync();
      var token = syntaxTree.GetRoot().FindToken(position).Parent;

      var currentText = token?.ToString()?.ToLower() ?? "";
      //CompletionHelper.AddSnippets(currentText, items);

      return items.Select(item => new Completion
      {
        Label = item.DisplayText,
        Kind = item.Tags.FirstOrDefault() ?? "Text",
        InsertText = item.Properties.ContainsKey("InsertText") ? item.Properties["InsertText"] : item.DisplayText,
        Tags = item.Tags.ToArray(),
        Documentation = item.InlineDescription,
      }).ToArray();
    }





    public async Task<string> GetFormattedDocument(string code, int tabSize, bool insertSpaces)
    {
      var document = UpdateCode(code);
      var workspace = _workspace;

      var root = await document.GetSyntaxRootAsync() as CompilationUnitSyntax;
      var formattedRoot = root;

      // Optional: Normalize first (remove if Formatter works without it)
      formattedRoot = formattedRoot.NormalizeWhitespace(indentation: new string(' ', tabSize), eol: "\r\n");

      OptionSet options = workspace.Options
          .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, !insertSpaces)
          .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, tabSize)
          .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, tabSize)
          .WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, "\r\n")
          .WithChangedOption(FormattingOptions.SmartIndent, LanguageNames.CSharp, FormattingOptions.IndentStyle.Smart)
          .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, true)
          .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, true)
          .WithChangedOption(CSharpFormattingOptions.NewLineForMembersInObjectInit, true)
          .WithChangedOption(CSharpFormattingOptions.SpacingAfterMethodDeclarationName, true)
          .WithChangedOption(CSharpFormattingOptions.SpaceWithinMethodCallParentheses, false)
          .WithChangedOption(CSharpFormattingOptions.IndentBlock, true)
          .WithChangedOption(CSharpFormattingOptions.IndentBraces, false);

      formattedRoot = (CompilationUnitSyntax)Formatter.Format(formattedRoot, workspace, options);
      var formattedCode = formattedRoot.ToFullString();

      return formattedCode;
    }





    public async Task<string> GetTooltip(string code, int line, int column)
    {
      var document = UpdateCode(code);

      var text = await document.GetTextAsync();
      var position = text.Lines[line - 1].Start + column - 1;

      var semanticModel = await document.GetSemanticModelAsync();
      var syntaxTree = await document.GetSyntaxTreeAsync();

      var root = await syntaxTree.GetRootAsync();
      var token = root.FindToken(position);

      // Get the symbol (method, class, variable, etc.)
      var symbol = semanticModel.GetSymbolInfo(token.Parent).Symbol;
      if (symbol != null)
      {
        return $"{symbol.Kind}: {symbol.Name}\n{symbol.ToDisplayString()}\n{symbol.GetDocumentationCommentXml()}";
      }

      return "No symbol information available";
    }





    public async Task<IEnumerable<ActionDiagnostic>> GetDiagnostics(string code)
    {
      var document = UpdateCode(code);

      var compilation = await document.Project.GetCompilationAsync();

      var diagnostics = compilation.GetDiagnostics();

      return diagnostics.Select(Convert);
    }





    public async Task<(IEnumerable<SignatureItem> signatures, int activeSignature, int activeParameter)> GetSignatures(string code, int line, int column)
    {
      var document = UpdateCode(code);
      var results = new List<SignatureItem>();
      var syntaxTree = await document.GetSyntaxTreeAsync();
      var text = await document.GetTextAsync();
      var position = text.Lines[line - 1].Start + column - 1;
      var semanticModel = await document.GetSemanticModelAsync();
      var root = await syntaxTree.GetRootAsync();
      var token = root.FindToken(position);

      var node = token.Parent;
      if (node is ArgumentListSyntax argNode)
        node = argNode.Parent; // Move up to ArgumentListSyntax

      var invocation = node?.FirstAncestorOrSelf<InvocationExpressionSyntax>();
      if (invocation == null)
        return (results, 0, 0);

      var symbolInfo = semanticModel.GetSymbolInfo(invocation);

      // Collect all possible method symbols (symbol + candidates)
      var methodSymbols = new List<IMethodSymbol>();
      if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
      {
        methodSymbols.Add(methodSymbol);
      }
      methodSymbols.AddRange(symbolInfo.CandidateSymbols.OfType<IMethodSymbol>());

      if (!methodSymbols.Any())
        return (results, 0, 0);

      // Build signature information for each overload
      foreach (var method in methodSymbols)
      {
        // Extract summary from XML documentation
        string xmlDoc = method.GetDocumentationCommentXml();
        string summary = default;
        if (!string.IsNullOrWhiteSpace(xmlDoc))
          summary = XmlHelper.ExtractSummaryFromXml(xmlDoc);

        var signature = new SignatureItem//SignatureInformation
        {
          Label = $"{method.Name}({string.Join(", ", method.Parameters.Select(p => $"{p.Type.Name} {p.Name}"))}): {method.ReturnType.Name}",
          Documentation = new MarkdownValue(summary ?? $"No XML documentation available for {method.ContainingType.Name}"),
          Parameters = method.Parameters.Select(p => new SignatureItemParameter//ParameterInformation
          {
            Label = $"{p.Type.Name} {p.Name}",
            Documentation = p.ToDisplayString()
          }).ToList()
        };
        results.Add(signature);
      }

      // Set active signature and parameter
      var activeSignature = 0; // Default to first overload; could be improved with argument matching
      var argumentList = invocation.ArgumentList;
      var separators = argumentList.Arguments.GetSeparators();
      int argumentPosition = separators.TakeWhile(s => s.Span.Start < position).Count();
      var activeParameter = Math.Min(argumentPosition, methodSymbols[activeSignature].Parameters.Length - 1);

      return (results, activeSignature, activeParameter);

      SignatureItem GenerateItem(IMethodSymbol symbol)
      {
        var label = symbol.Name + "(";
        var parameters = new List<SignatureItemParameter>();

        // Print method parameters
        for (int i = 0; i < symbol.Parameters.Length; i++)
        {
          var param = symbol.Parameters[i];
          parameters.Add(new SignatureItemParameter()
          {
            Label = param.Name,
            Documentation = param.GetDocumentationCommentXml()
          });
          label = $"{(i > 0 ? ", " : "")}{param.Type} {param.Name}";
        }
        label += ")";

        var result = new SignatureItem()
        {
          Label = label,
          Documentation = new MarkdownValue(symbol.GetDocumentationCommentXml()),
          Parameters = parameters,
        };

        return result;
      }
    }






    public ActionDiagnostic Convert(Diagnostic diagnostic)
    {
      var location = diagnostic.Location.GetLineSpan();

      return new ActionDiagnostic
      {
        Message = diagnostic.GetMessage(),
        StartLineNumber = location.StartLinePosition.Line + 1,
        StartColumn = location.StartLinePosition.Character + 1,
        EndLineNumber = location.EndLinePosition.Line + 1,
        EndColumn = location.EndLinePosition.Character + 1,
        Severity = diagnostic.Severity.ToString(),
      };
    }


  }
}
