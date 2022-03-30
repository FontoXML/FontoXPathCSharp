using System.Xml;
using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;

namespace FontoXPathCSharp.EvaluationUtils;

public class EvaluationContext<TSelector>
{
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
    }
}