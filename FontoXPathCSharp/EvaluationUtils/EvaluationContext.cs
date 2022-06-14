using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value.Types;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc =
    System.Func<FontoXPathCSharp.Types.LexicalQualifiedName, int, FontoXPathCSharp.Types.ResolvedQualifiedName>;

namespace FontoXPathCSharp.EvaluationUtils;

public class EvaluationContext<TSelector>
{
    public EvaluationContext(
        TSelector expression,
        IExternalValue? contextItem,
        IDomFacade? domFacade,
        Dictionary<string, IExternalValue>? variables,
        Options? externalOptions,
        CompilationOptions compilationOptions)
    {
        variables ??= new Dictionary<string, IExternalValue>();

        var internalOptions = externalOptions != null
            ? new Options
            {
                Logger = externalOptions.Logger ?? Console.WriteLine,
                DocumentWriter = externalOptions.DocumentWriter,
                ModuleImports = externalOptions.ModuleImports,
                NamespaceResolver = externalOptions.NamespaceResolver,
                FunctionNameResolver = externalOptions.FunctionNameResolver,
                NodesFactory = externalOptions.NodesFactory
            }
            : new Options
            {
                Logger = Console.WriteLine,
                DocumentWriter = null,
                ModuleImports = new Dictionary<string, string>(),
                NamespaceResolver = null,
                FunctionNameResolver = null,
                NodesFactory = null
            };

        var wrappedDomFacade = createWrappedDomFacade(domFacade);

        var moduleImports = internalOptions.ModuleImports ?? new Dictionary<string, string>();

        var namespaceResolver = internalOptions.NamespaceResolver ?? createDefaultNamespaceResolver(contextItem);

        var defaultFunctionNamespaceURI = externalOptions?.DefaultFunctionNamespaceUri ??
                                          BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri();

        var functionNameResolver = internalOptions.FunctionNameResolver ??
                                   createDefaultFunctionNameResolver(defaultFunctionNamespaceURI);

        var expressionAndStaticContext = CompileXPath.StaticallyCompileXPath(expression, compilationOptions,
            namespaceResolver, variables, moduleImports, defaultFunctionNamespaceURI, functionNameResolver);

        var contextSequence = contextItem != null
            ? adaptValueToSequence(wrappedDomFacade, contextItem)
            : SequenceFactory.CreateEmpty();

        //    var nodesFactory = internalOptions.NodesFactory != null && compilationOptions.AllowXQuery
        //        ? wrapExternalDocumentWriter(internalOptions.DocumentWriter)
        //        : domBackedDocumentWriter;

        DynamicContext = new DynamicContext(contextSequence.First(), 0);
    }

    public DynamicContext DynamicContext { get; }

    public ExecutionParameters? ExecutionParameters { get; }

    public AbstractExpression Expression { get; }

    private static DomFacade.DomFacade createWrappedDomFacade(IDomFacade? domFacade)
    {
        if (domFacade != null) return new DomFacade.DomFacade(domFacade);
        throw new Exception("External Dom Facade not implemented yet");
    }

    private static ISequence adaptValueToSequence(DomFacade.DomFacade domFacade, IExternalValue value,
        SequenceType? expectedType = null)
    {
        throw new NotImplementedException("adaptValueToSequence");
    }

    private static NamespaceResolverFunc createDefaultNamespaceResolver(IExternalValue? contextItem)
    {
        throw new NotImplementedException("Default Namespace Resolver not implemented yet");
    }

    private FunctionNameResolverFunc createDefaultFunctionNameResolver(string defaultFunctionNamespaceUri)
    {
        throw new NotImplementedException("Default Function Name Resolver not implemented yet");
    }
}