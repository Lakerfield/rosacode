
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
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
            var root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync();
            bool alreadyHasUsing = UsingHelper.HasUsing(root, ns);

            if (alreadyHasUsing)
              continue;

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

        // Interface implementation logic for both regular and generic interfaces
        var classDeclaration = token.Parent.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration != null)
        {
          var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
          var baseList = classDeclaration.BaseList?.Types;
          if (baseList != null)
          {
            foreach (var baseType in baseList)
            {
              // Handle both regular and generic interfaces properly
              var baseTypeSyntax = baseType.Type;

              TypeSyntax effectiveType = baseTypeSyntax;
              if (effectiveType is NullableTypeSyntax nullable)
                effectiveType = nullable.ElementType;
              // -------------------------------------------

              string interfaceName = "";
              var typeArguments = new List<string>();

              if (effectiveType is GenericNameSyntax genericName)
              {
                interfaceName = genericName.Identifier.Text;
                typeArguments = genericName.TypeArgumentList.Arguments
                                         .Select(a => a.ToString()).ToList();
              }
              else if (effectiveType is IdentifierNameSyntax id)
              {
                interfaceName = id.Identifier.Text;
              }
              else if (effectiveType is QualifiedNameSyntax qualified)
              {
                var right = qualified.Right;
                if (right is GenericNameSyntax g)               // ← pattern match
                {
                  interfaceName = g.Identifier.Text;          // "IEnumerable"
                  typeArguments = g.TypeArgumentList
                                   .Arguments
                                   .Select(a => a.ToString())
                                   .ToList();                // ["string"]
                }
                else interfaceName = right.ToString();
              }
              else
              {
                interfaceName = effectiveType.ToString();
              }
              INamedTypeSymbol interfaceSymbol = null;
              if (classSymbol == null || classSymbol.Interfaces.All(i => i.Name != interfaceName))
              {
                foreach (var reference in compilation.References)
                {
                  var assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                  if (assembly != null)
                  {
                    // Look for interfaces with matching name and type argument count
                    var allTypes = assembly.GlobalNamespace.GetNamespaceMembers()
                        .SelectMany(ns => GetAllTypes(ns));
                    interfaceSymbol = allTypes
                        .FirstOrDefault(t =>
                            t.Name == interfaceName &&
                            t.TypeKind == TypeKind.Interface &&
                            (typeArguments.Count == 0 ||
                             t is INamedTypeSymbol namedType &&
                             namedType.TypeArguments.Length == typeArguments.Count))
                        as INamedTypeSymbol;
                    if (interfaceSymbol != null)
                    {
                      break;
                    }
                  }
                }
              }
              else
              {
                // Find existing interface implementation in class
                interfaceSymbol = classSymbol.Interfaces.FirstOrDefault(i => i.Name == interfaceName);
              }
              if (interfaceSymbol != null)
              {
                HashSet<string> requiredNamespaces = new HashSet<string>();
                var ifaceNs = interfaceSymbol.ContainingNamespace?.ToDisplayString();
                if (!string.IsNullOrEmpty(ifaceNs))
                  requiredNamespaces.Add(ifaceNs);
                var interfacesToInspect = interfaceSymbol.AllInterfaces.Concat(new[] { interfaceSymbol });
                foreach (var iface in interfacesToInspect)
                {
                  var baseIfaceNs = iface.ContainingNamespace?.ToDisplayString();
                  if (!string.IsNullOrEmpty(baseIfaceNs))
                    requiredNamespaces.Add(baseIfaceNs);
                  foreach (var member in iface.GetMembers())
                  {
                    switch (member)
                    {
                      case IMethodSymbol m:
                        // Return type
                        UsingHelper.AddNamespacesFromType(m.ReturnType, requiredNamespaces);
                        // Parameter types
                        foreach (var p in m.Parameters)
                          UsingHelper.AddNamespacesFromType(p.Type, requiredNamespaces);
                        break;
                      case IPropertySymbol p:
                        UsingHelper.AddNamespacesFromType(p.Type, requiredNamespaces);
                        break;
                      case IEventSymbol e:
                        UsingHelper.AddNamespacesFromType(e.Type, requiredNamespaces);
                        break;
                    }
                  }
                }
                var enumeratorReturnType = UsingHelper.GetNonGenericEnumeratorReturnType(interfaceSymbol, compilation);
                if (enumeratorReturnType != null)
                {
                  UsingHelper.AddNamespacesFromType(enumeratorReturnType, requiredNamespaces);
                }
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
                      $"Implement interface '{interfaceSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}'",
                      async cancellationToken =>
                      {
                        var root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken);
                        var editor = new SyntaxEditor(root, document.Project.Solution.Workspace);
                        // Collect new usings to add
                        var newUsings = new List<UsingDirectiveSyntax>();
                        foreach (var ns in requiredNamespaces)
                        {
                          // Skip if the file already has this using.
                          if (root.Usings.Any(u => u.Name.ToString() == ns))
                            continue;
                          // Build the new using-directive.
                          var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns))
                                                      .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
                          newUsings.Add(newUsing);
                        }
                        // Insert all new usings at once
                        if (newUsings.Count > 0)
                        {
                          var lastUsing = root.Usings.LastOrDefault();
                          if (lastUsing != null)
                          {
                            editor.InsertAfter(lastUsing, newUsings);
                          }
                          else
                          {
                            var firstMember = root.Members.FirstOrDefault();
                            if (firstMember != null)
                            {
                              editor.InsertBefore(firstMember, newUsings);
                            }
                            else
                            {
                              // Empty file - replace root with usings added
                              editor.ReplaceNode(root, root.WithUsings(SyntaxFactory.List(newUsings)));
                            }
                          }
                        }
                        // Fix base type if qualified with wrong namespace
                        string correctNs = interfaceSymbol.ContainingNamespace?.ToDisplayString() ?? "";
                        if (baseTypeSyntax is QualifiedNameSyntax qualifiedSyntax)
                        {
                          string writtenNs = qualifiedSyntax.Left.ToString();
                          if (writtenNs != correctNs)
                          {
                            var correctTypeSyntax = SyntaxFactory.QualifiedName(
                                SyntaxFactory.ParseName(correctNs),
                                qualifiedSyntax.Right);
                            editor.ReplaceNode(baseTypeSyntax, correctTypeSyntax);
                          }
                        }
                        var classNode = editor.OriginalRoot
                              .DescendantNodes()
                              .OfType<ClassDeclarationSyntax>()
                              .First(cd => cd.Identifier.ValueText == classDeclaration.Identifier.ValueText);
                        // Special handling for interfaces with type parameters to generate proper GetEnumerator method
                        if (typeArguments.Count > 0)
                        {
                          // Check if this interface has a GetEnumerator method with no parameters
                          var getEnumeratorMethod = allInterfaceMembers
                                .OfType<IMethodSymbol>()
                                .FirstOrDefault(m => m.Name == "GetEnumerator" && m.Parameters.Length == 0);
                          if (getEnumeratorMethod != null)
                          {
                            var genericTypeArgument = typeArguments[0]; // For interfaces like IEnumerable<string>, this is "string"
                                                                        // Generate IEnumerator<T> GetEnumerator()
                            var enumeratorMethod = SyntaxFactory.MethodDeclaration(
                                  SyntaxFactory.ParseTypeName($"IEnumerator<{genericTypeArgument}>"),
                                  "GetEnumerator")
                                  .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                  .WithBody(SyntaxFactory.Block(
                                      SyntaxFactory.ThrowStatement(
                                          SyntaxFactory.ObjectCreationExpression(
                                              SyntaxFactory.ParseName("NotImplementedException"))
                                          .WithArgumentList(SyntaxFactory.ArgumentList()))));
                            editor.AddMember(classNode, enumeratorMethod);
                            // Generate IEnumerator IEnumerable.GetEnumerator() - explicit interface implementation (no 'public' keyword)
                            var enumerableGetEnumeratorMethod = SyntaxFactory.MethodDeclaration(
                                  SyntaxFactory.ParseTypeName("IEnumerator"),
                                  "IEnumerable.GetEnumerator")
                                  .WithModifiers(SyntaxFactory.TokenList()) // No modifiers for explicit interface implementation
                                  .WithBody(SyntaxFactory.Block(
                                      SyntaxFactory.ReturnStatement(
                                          SyntaxFactory.InvocationExpression(
                                              SyntaxFactory.IdentifierName("GetEnumerator")))));
                            editor.AddMember(classNode, enumerableGetEnumeratorMethod);
                          }
                        }
                        // General case for other interface members
                        foreach (var member in allInterfaceMembers)
                        {
                          if (member is IMethodSymbol method)
                          {
                            // Handle generic method signatures properly
                            var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                            // Skip the GetEnumerator we already handled above
                            if (method.Name == "GetEnumerator" && method.Parameters.Length == 0 && typeArguments.Count > 0)
                            {
                              // This is already handled above for generic interfaces
                              continue;
                            }
                            var methodDecl = SyntaxFactory.MethodDeclaration(
                                  SyntaxFactory.ParseTypeName(returnType),
                                  method.Name)
                                  .WithParameterList(SyntaxFactory.ParameterList(
                                      SyntaxFactory.SeparatedList(method.Parameters.Select(p =>
                                          SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                          .WithType(
                                              SyntaxFactory.ParseTypeName(
                                                  p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)))))))
                                  .WithBody(SyntaxFactory.Block(
                                      SyntaxFactory.ThrowStatement(
                                          SyntaxFactory.ObjectCreationExpression(
                                              SyntaxFactory.ParseName("NotImplementedException"))
                                          .WithArgumentList(SyntaxFactory.ArgumentList()))))
                                  .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
                            editor.AddMember(classNode, methodDecl);
                          }
                          else if (member is IPropertySymbol property)
                          {
                            // Handle properties
                            var propertyType = property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                            var propertyDecl = SyntaxFactory.PropertyDeclaration(
                                  SyntaxFactory.ParseTypeName(propertyType),
                                  property.Name)
                                  .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                  .WithAccessorList(SyntaxFactory.AccessorList(
                                      SyntaxFactory.List(new[]
                                      {
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                          })));
                            editor.AddMember(classNode, propertyDecl);
                          }
                          else if (member is IEventSymbol eventSymbol)
                          {
                            // Handle events
                            var eventType = eventSymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                            var eventDecl = SyntaxFactory.EventDeclaration(
                                  SyntaxFactory.ParseTypeName(eventType),
                                  eventSymbol.Name)
                                  .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
                            editor.AddMember(classNode, eventDecl);
                          }
                        }
                        var newRoot = editor.GetChangedRoot();
                        return document.WithSyntaxRoot(newRoot).Project.Solution;
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
