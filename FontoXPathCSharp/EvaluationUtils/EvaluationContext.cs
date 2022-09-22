using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.NodesFactory;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc =
    System.Func<FontoXPathCSharp.Types.LexicalQualifiedName, int, FontoXPathCSharp.Types.ResolvedQualifiedName?>;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.EvaluationUtils;

public class EvaluationContext<TSelector, TNode>
{
    public EvaluationContext(
        TSelector expression,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade,
        Dictionary<string, AbstractValue?> variables,
        Options<TNode>? externalOptions,
        CompilationOptions compilationOptions)
    {
        var internalOptions = externalOptions != null
            ? new Options<TNode>(externalOptions.NamespaceResolver)
            {
                DocumentWriter = externalOptions.DocumentWriter,
                ModuleImports = externalOptions.ModuleImports,
                FunctionNameResolver = externalOptions.FunctionNameResolver,
                NodesFactory = externalOptions.NodesFactory,
                Logger = externalOptions.Logger ?? Console.WriteLine
            }
            : new Options<TNode>(_ => null)
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

        var namespaceResolver = internalOptions.NamespaceResolver ??
                                CreateDefaultNamespaceResolver(contextItem, wrappedDomFacade);

        var defaultFunctionNamespaceUri = externalOptions?.DefaultFunctionNamespaceUri ??
                                          BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri();

        var functionNameResolver = internalOptions.FunctionNameResolver ??
                                   CreateDefaultFunctionNameResolver(defaultFunctionNamespaceUri);

        var expressionAndStaticContext = CompileXPath<TSelector, TNode>.StaticallyCompileXPath(
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

        INodesFactory<TNode> nodesFactory = null;
        // var nodesFactory = internalOptions.NodesFactory != null && compilationOptions.AllowXQuery
        //     ? WrapExternalDocumentWriter(internalOptions.DocumentWriter)
        //     : DomBackedDocumentWriter;

        IDocumentWriter<TNode> documentWriter = null;

        var xmlSerializer = internalOptions.XmlSerializer;

        var variableBindings = variables.Keys.Aggregate(
            new Dictionary<string, Func<ISequence>>(),
            (typedVariableByName, variableName) =>
            {
                var variable = variables[variableName];
                // if (variable && IS_XPATH_VALUE_SYMBOL in variable) {
                //     // If this symbol is present, the value has already undergone type conversion.
                //     const castedObject = variable as TypedExternalValue;
                //     typedVariableByName[generateGlobalVariableBindingName(variableName)] = () => {
                //         return sequenceFactory.create(castedObject.convertedValue);
                //     };
                // } else {
                //     typedVariableByName[generateGlobalVariableBindingName(variableName)] = () => {
                //         // The value is not converted yet. Do it just in time.
                //         return adaptJavaScriptValueToSequence(
                //             wrappedDomFacade,
                //             variables[variableName]
                //         );
                //     };
                // }
                return typedVariableByName;
            }
        );

        var dynamicContext = new DynamicContext(
            contextSequence.First(),
            0,
            contextSequence,
            variableBindings
        );

        ExecutionParameters = new ExecutionParameters<TNode>(
            compilationOptions.Debug,
            compilationOptions.DisableCache,
            wrappedDomFacade,
            externalOptions.CurrentContext,
            nodesFactory,
            documentWriter,
            internalOptions.Logger,
            xmlSerializer
        );

        foreach (var binding in expressionAndStaticContext.StaticContext.GetVariableBindings())
            if (!variableBindings.ContainsKey(binding))
                variableBindings[binding] = () =>
                    expressionAndStaticContext.StaticContext.GetVariableDeclaration(binding)(
                        dynamicContext,
                        ExecutionParameters
                    );

        Expression = expressionAndStaticContext.Expression;
    }

    public DynamicContext DynamicContext { get; }

    public ExecutionParameters<TNode> ExecutionParameters { get; }

    public AbstractExpression<TNode> Expression { get; }

    private static DomFacade<TNode> CreateWrappedDomFacade(IDomFacade<TNode>? domFacade)
    {
        if (domFacade != null) return new DomFacade<TNode>(domFacade);

        throw new NotImplementedException(
            "EvaluationContext.CreateWrappedDomFacade: External Dom Facade not implemented yet");
    }

    private static ISequence AdaptValueToSequence(
        DomFacade<TNode> domFacade,
        AbstractValue value,
        SequenceType? expectedType = null)
    {
        return new SingletonSequence(new NodeValue<TNode>(value.GetAs<NodeValue<TNode>>().Value, domFacade));
    }

    private static NamespaceResolverFunc CreateDefaultNamespaceResolver(
        AbstractValue? contextItem,
        DomFacade<TNode>? domFacade
    )
    {
        if (contextItem == null || domFacade == null || !contextItem.GetValueType().IsSubtypeOf(ValueType.Node))
            return _ => null;

        //TODO: Fix this stuff.
        Console.WriteLine("CreateDefaultNamespaceResolver is not finished properly.");
        return _ => null;
        // var node = contextItem.GetAs<NodeValue<TNode>>().Value;
        //
        // return prefix => domFacade.LookupNamespaceUri(node, prefix);
    }

    private static FunctionNameResolverFunc CreateDefaultFunctionNameResolver(string defaultFunctionNamespaceUri)
    {
        return (lexicalQualifiedName, arity) => lexicalQualifiedName.Prefix == null
            ? new ResolvedQualifiedName(lexicalQualifiedName.LocalName, defaultFunctionNamespaceUri)
            : null;
    }
}