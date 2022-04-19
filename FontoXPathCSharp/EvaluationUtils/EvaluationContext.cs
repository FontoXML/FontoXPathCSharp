using System.Xml;
using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.DomFacade;

namespace FontoXPathCSharp.EvaluationUtils;

public class EvaluationContext<TSelector>
{
    private DynamicContext _dynamicContext;
    private ExecutionParameters _executionParameters;
    private AbstractExpression _abstractExpression;
    
    public EvaluationContext(
        TSelector selector,
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

        DomFacade wrappedDomFacade = new DomFacade(domFacade);
        
        
    }
    public DynamicContext DynamicContext => _dynamicContext;
    public ExecutionParameters ExecutionParameters => _executionParameters;
    public AbstractExpression Expression => _abstractExpression;
}