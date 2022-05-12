using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc = System.Func<FontoXPathCSharp.Types.LexicalQualifiedName, int, FontoXPathCSharp.Types.ResolvedQualifiedName>;

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

        var wrappedDomFacade = CreateWrappedDomFacade(domFacade);

        var moduleImports = internalOptions.ModuleImports;

        var namespaceResolver = internalOptions.NamespaceResolver ?? CreateDefaultNamespaceResolver(contextItem);

        var defaultFunctionNamespaceUri = externalOptions.DefaultFunctionNamespaceUri ??
                                          BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri();

        var functionNameResolver = internalOptions.FunctionNameResolver ??
                                   CreateDefaultFunctionNameResolver(defaultFunctionNamespaceUri);

        var expressionAndStaticContext = CompileXPath.StaticallyCompileXPath(expression, compilationOptions,
            namespaceResolver, variables, moduleImports, defaultFunctionNamespaceUri, functionNameResolver);
    }

    public DynamicContext DynamicContext { get; }

    public ExecutionParameters ExecutionParameters { get; }

    public AbstractExpression Expression { get; }

    private static DomFacade.DomFacade CreateWrappedDomFacade(IDomFacade? domFacade)
    {
        if (domFacade != null) return new DomFacade.DomFacade(domFacade);

        throw new Exception("External Dom Facade not implemented yet");
    }

    private static NamespaceResolverFunc CreateDefaultNamespaceResolver(IExternalValue contextItem)
    {
        throw new NotImplementedException("Default Namespace Resolver not implemented yet");
    }

    private static FunctionNameResolverFunc CreateDefaultFunctionNameResolver(string defaultFunctionNamespaceUri)
    {
        throw new NotImplementedException("Default Function Name Resolver not implemented yet");
    }
}