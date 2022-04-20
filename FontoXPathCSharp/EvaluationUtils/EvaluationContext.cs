using System.Xml;
using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.DomFacade;


using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc = System.Func<LexicalQualifiedName, int, ResolvedQualifiedName>;

namespace FontoXPathCSharp.EvaluationUtils;



public class EvaluationContext<TSelector>
{
    private DynamicContext _dynamicContext;
    private ExecutionParameters _executionParameters;
    private AbstractExpression _abstractExpression;
    
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
                NodesFactory = null,
            };

        var wrappedDomFacade = createWrappedDomFacade(domFacade);

        var moduleImports = internalOptions.ModuleImports;

        var namespaceResolver = internalOptions.NamespaceResolver ?? createDefaultNamespaceResolver(contextItem);

        var defaultFunctionNamespaceURI = externalOptions.DefaultFunctionNamespaceUri ?? BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceURI();

        var functionNameResolver = internalOptions.FunctionNameResolver ?? createDefaultFunctionNameResolver(defaultFunctionNamespaceURI);
        
        var expressionAndStaticContext = CompileXPath.StaticallyCompileXPath(expression, compilationOptions, namespaceResolver, variables, moduleImports, defaultFunctionNamespaceURI, functionNameResolver);

    }

    private DomFacade.DomFacade createWrappedDomFacade(IDomFacade? domFacade)
    {
        if (domFacade != null)
        {
            return new DomFacade.DomFacade(domFacade);
        }
        throw new Exception("External Dom Facade not implemented yet");
    }
    private static NamespaceResolverFunc createDefaultNamespaceResolver(IExternalValue contextItem)
    {
        throw new NotImplementedException("Default Namespace Resolver not implemented yet");
    }
    
    private FunctionNameResolverFunc createDefaultFunctionNameResolver(string defaultFunctionNamespaceUri)
    {
        throw new NotImplementedException("Default Function Name Resolver not implemented yet");
    }
    public DynamicContext DynamicContext => _dynamicContext;
    public ExecutionParameters ExecutionParameters => _executionParameters;
    public AbstractExpression Expression => _abstractExpression;
}