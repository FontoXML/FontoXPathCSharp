using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.NodesFactory;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.EvaluationUtils;

public class EvaluationContext<TSelector, TNode> where TSelector : notnull where TNode : notnull
{
    public EvaluationContext(
        TSelector expression,
        object? contextItem,
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
                NamespaceResolver = _ => null,
                FunctionNameResolver = null,
                NodesFactory = null
            };

        var wrappedDomFacade = CreateWrappedDomFacade(domFacade);

        var moduleImports = internalOptions.ModuleImports ?? new Dictionary<string, string>();

        var namespaceResolver = internalOptions.NamespaceResolver;

        var defaultFunctionNamespaceUri = externalOptions?.DefaultFunctionNamespaceUri ??
                                          BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri();

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
            ? AdaptToValues<TNode>.AdaptValueToSequence(wrappedDomFacade, contextItem)
            : SequenceFactory.CreateEmpty();

        INodesFactory<TNode>? nodesFactory = null;
        // var nodesFactory = internalOptions.NodesFactory != null && compilationOptions.AllowXQuery
        //     ? WrapExternalDocumentWriter(internalOptions.DocumentWriter)
        //     : DomBackedDocumentWriter;

        IDocumentWriter<TNode>? documentWriter = null;

        var xmlSerializer = internalOptions.XmlSerializer;

        var variableBindings = variables.Keys.Aggregate(
            new Dictionary<string, Func<ISequence>>(),
            (typedVariableByName, variableName) =>
            {
                typedVariableByName[GenerateGlobalVariableBindingName(variableName)] =
                    variables.ContainsKey(variableName)
                        ? () => SequenceFactory.CreateFromValue(variables[variableName])
                        : () => AdaptToValues<TNode>.AdaptValueToSequence(
                            wrappedDomFacade,
                            variables[variableName]
                        );
                return typedVariableByName;
            }
        );

        DynamicContext = new DynamicContext(
            contextSequence.First(),
            0,
            contextSequence,
            variableBindings
        );

        ExecutionParameters = new ExecutionParameters<TNode>(
            compilationOptions.Debug,
            compilationOptions.DisableCache,
            wrappedDomFacade,
            externalOptions!.CurrentContext,
            nodesFactory,
            documentWriter,
            internalOptions.Logger,
            xmlSerializer
        );

        foreach (var binding in expressionAndStaticContext.StaticContext.GetVariableBindings())
            if (!variableBindings.ContainsKey(binding))
                variableBindings[binding] = () =>
                    expressionAndStaticContext.StaticContext.GetVariableDeclaration(binding)!(
                        DynamicContext,
                        ExecutionParameters
                    );

        Expression = expressionAndStaticContext.Expression;
    }

    public DynamicContext DynamicContext { get; }

    public ExecutionParameters<TNode> ExecutionParameters { get; }

    public AbstractExpression<TNode> Expression { get; }

    private static string GenerateGlobalVariableBindingName(string variableName)
    {
        return $"Q{{}}{variableName}[0]";
    }

    private static DomFacade<TNode> CreateWrappedDomFacade(IDomFacade<TNode>? domFacade)
    {
        if (domFacade != null) return new DomFacade<TNode>(domFacade);

        throw new NotImplementedException(
            "EvaluationContext.CreateWrappedDomFacade: External Dom Facade not implemented yet");
    }

    private static FunctionNameResolver CreateDefaultFunctionNameResolver(string defaultFunctionNamespaceUri)
    {
        // Second parameter is arity.
        return (lexicalQualifiedName, _) => lexicalQualifiedName.Prefix == null
            ? new ResolvedQualifiedName(lexicalQualifiedName.LocalName, defaultFunctionNamespaceUri)
            : null;
    }
}