using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Expressions.Functions;

public record ParameterDescription(QName ParameterName, ParameterType ParameterType);

public class InlineFunction<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly PossiblyUpdatingExpression<TNode> _functionBody;
    private readonly QName[] _parameterNames;
    private readonly ParameterType[] _parameterTypes;
    private readonly SequenceType _returnType;
    private string[] _parameterBindingNames;

    public InlineFunction(
        ParameterDescription[] parameterDescriptions,
        SequenceType returnType,
        PossiblyUpdatingExpression<TNode> functionBody) : base(
        new Specificity(SpecificityKind.External, 1),
        new AbstractExpression<TNode>[] { functionBody },
        new OptimizationOptions())
    {
        _parameterNames = parameterDescriptions.Select(desc => desc.ParameterName).ToArray();
        _parameterTypes = parameterDescriptions.Select(desc => desc.ParameterType).ToArray();

        _parameterBindingNames = Array.Empty<string>();
        _returnType = returnType;
        _functionBody = functionBody;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        FunctionSignature<ISequence, TNode> executeFunction =
            (_, _, _, parameters) =>
            {
                // Since functionCall already does typechecking, we do not have to do it here
                var scopedDynamicContext = dynamicContext?
                    .ScopeWithFocus(-1, null, SequenceFactory.CreateEmpty())
                    .ScopeWithVariableBindings(_parameterBindingNames.Reduce(
                        new Dictionary<string, Func<ISequence>>(),
                        (paramByName, bindingName, i) =>
                        {
                            paramByName[bindingName] = ISequence.CreateDoublyIterableSequence(parameters[i]);
                            return paramByName;
                        })
                    );
                return _functionBody.EvaluateMaybeStatically(
                    scopedDynamicContext,
                    executionParameters
                );
            };

        var functionItem = new FunctionValue<ISequence, TNode>(
            _parameterTypes,
            _parameterTypes.Length,
            "dynamic-function",
            "",
            _returnType,
            executeFunction,
            true,
            _functionBody.IsUpdating
        );
        return SequenceFactory.CreateFromValue(functionItem);
    }

    public override void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        staticContext.IntroduceScope();
        _parameterBindingNames = _parameterNames.Select(name =>
        {
            var namespaceUri = name.NamespaceUri;
            var prefix = name.Prefix;
            var localName = name.LocalName;

            if (namespaceUri == null && prefix != "*")
                namespaceUri = staticContext.ResolveNamespace(prefix);
            return staticContext.RegisterVariable(namespaceUri, localName);
        }).ToArray();

        _functionBody.PerformStaticEvaluation(staticContext);
        staticContext.RemoveScope();

        if (_functionBody.IsUpdating) throw new Exception("Not implemented: inline functions can not yet be updating.");
    }
}