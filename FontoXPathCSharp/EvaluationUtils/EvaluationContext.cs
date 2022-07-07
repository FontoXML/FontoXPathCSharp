using System.Xml;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc =
    System.Func<FontoXPathCSharp.Types.LexicalQualifiedName, int, FontoXPathCSharp.Types.ResolvedQualifiedName?>;

namespace FontoXPathCSharp.EvaluationUtils;

public class EvaluationContext<TSelector>
{
    public EvaluationContext(
        TSelector expression,
        XmlNode? contextItem,
        IDomFacade? domFacade,
        Dictionary<string, AbstractValue>? variables,
        Options? externalOptions,
        CompilationOptions compilationOptions)
    {
        variables ??= new Dictionary<string, AbstractValue>();

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

        var moduleImports = internalOptions.ModuleImports ?? new Dictionary<string, string>();

        var namespaceResolver = internalOptions.NamespaceResolver ?? CreateDefaultNamespaceResolver(contextItem);

        var defaultFunctionNamespaceUri = externalOptions?.DefaultFunctionNamespaceUri ??
                                          BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri();

        var functionNameResolver = internalOptions.FunctionNameResolver ??
                                   CreateDefaultFunctionNameResolver(defaultFunctionNamespaceUri);

        var expressionAndStaticContext = CompileXPath.StaticallyCompileXPath(
            expression,
            compilationOptions,
            namespaceResolver,
            variables,
            moduleImports,
            defaultFunctionNamespaceUri,
            functionNameResolver
        );

        var contextSequence = contextItem != null
            ? AdaptValueToSequence(wrappedDomFacade, contextItem)
            : SequenceFactory.CreateEmpty();

        // var nodesFactory = internalOptions.NodesFactory != null && compilationOptions.AllowXQuery
        //     ? wrapExternalDocumentWriter(internalOptions.DocumentWriter)
        //     : domBackedDocumentWriter;

        DynamicContext = new DynamicContext(contextSequence.First(), 0, contextSequence);
        ExecutionParameters = new ExecutionParameters(contextItem);
        Expression = expressionAndStaticContext.Expression;
    }

    public DynamicContext DynamicContext { get; }

    public ExecutionParameters? ExecutionParameters { get; }

    public AbstractExpression Expression { get; }

    private static DomFacade.DomFacade CreateWrappedDomFacade(IDomFacade? domFacade)
    {
        if (domFacade != null) return new DomFacade.DomFacade(domFacade);
        return new DomFacade.DomFacade();
        // throw new Exception("External Dom Facade not implemented yet");
    }

    private static ISequence AdaptValueToSequence(DomFacade.DomFacade domFacade, XmlNode value,
        SequenceType? expectedType = null)
    {
        return new SingletonSequence(new NodeValue(value));
    }

    private static NamespaceResolverFunc CreateDefaultNamespaceResolver(XmlNode? contextItem)
    {
        if (contextItem == null) return _ => null;

        return prefix => prefix + contextItem.NamespaceURI;
    }

    private static FunctionNameResolverFunc CreateDefaultFunctionNameResolver(string defaultFunctionNamespaceUri)
    {
        return (lexicalQualifiedName, arity) => lexicalQualifiedName.Prefix == null
            ? new ResolvedQualifiedName(lexicalQualifiedName.LocalName, defaultFunctionNamespaceUri)
            : null;
    }
}