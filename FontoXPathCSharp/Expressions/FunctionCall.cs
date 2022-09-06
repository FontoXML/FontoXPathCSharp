using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class FunctionCall<TNode> : PossiblyUpdatingExpression<TNode>
{
    private readonly AbstractExpression<TNode>[] _argumentExpressions;
    private readonly int _callArity;
    private readonly AbstractExpression<TNode> _functionReferenceExpression;
    private FunctionValue<ISequence, TNode>? _functionReference;
    private StaticContext<TNode>? _staticContext;

    public FunctionCall(
        AbstractExpression<TNode> functionReferenceExpression,
        AbstractExpression<TNode>[] args) : base(
        new Specificity(new Dictionary<SpecificityKind, int> { { SpecificityKind.External, 1 } }),
        new[] { functionReferenceExpression }.Concat(args).ToArray(),
        new OptimizationOptions(false))
    {
        _argumentExpressions = args;
        _callArity = args.Length;
        _functionReferenceExpression = functionReferenceExpression;
        _staticContext = null;
    }

    public override ISequence PerformFunctionalEvaluation(DynamicContext? dynamicContext,
        ExecutionParameters<TNode> executionParameters, SequenceCallback[] createArgumentSequences)
    {
        if (_functionReference != null)
            return _functionReference.Value(
                dynamicContext,
                executionParameters,
                null,
                _argumentExpressions.Select(x => x.Evaluate(dynamicContext, executionParameters)).ToArray());


        var createFunctionReferenceSequence = createArgumentSequences[0];
        createArgumentSequences = createArgumentSequences.Skip(1).ToArray();

        var sequence = createFunctionReferenceSequence(dynamicContext);
        if (!sequence.IsSingleton())
            throw new XPathException(
                "XPTY0004",
                "Expected base expression of a function call to evaluate to a sequence of single function item");

        return sequence.MapAll(item =>
        {
            var functionItem = ValidateFunctionItem<AbstractValue>(item[0], _callArity);
            if (functionItem.IsUpdating)
                throw new XPathException(
                    "XUDY0038",
                    "The function returned by the PrimaryExpr of a dynamic function invocation can not be an updating function");

            return CallFunction(
                functionItem,
                functionItem.Value,
                dynamicContext,
                executionParameters,
                createArgumentSequences,
                _staticContext
            );
        });

        throw new XPathException(
            "XUDY0038",
            "Expected base expression of a function call to evaluate to a sequence of single function item");
    }

    private ISequence CallFunction(
        AbstractValue functionItem,
        MulticastDelegate functionItemValue,
        DynamicContext dynamicContext,
        ExecutionParameters<TNode> executionParameters,
        SequenceCallback[] createArgumentSequences,
        StaticContext<TNode> staticContext)
    {
        throw new NotImplementedException("Function calls not implemented yet.");
    }

    public override void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        _staticContext = staticContext.Clone();

        base.PerformStaticEvaluation(staticContext);

        if (_functionReferenceExpression.CanBeStaticallyEvaluated)
        {
            var functionRefSequence = _functionReferenceExpression.EvaluateMaybeStatically(null, null);
            if (!functionRefSequence.IsSingleton()) throw new XPathException("XPTY0004", "");

            _functionReference = ValidateFunctionItem<ISequence>(functionRefSequence.First()!, _callArity);

            // TODO: check if function reference is updating
        }
    }

    private static FunctionValue<T, TNode> ValidateFunctionItem<T>(AbstractValue item, int callArity)
    {
        var functionItem = item.GetAs<FunctionValue<T, TNode>>();

        if (functionItem == null)
            throw new XPathException("XPTY0004",
                "Expected base expression of a function call to evaluate to a sequence of single function item");

        if (functionItem.Arity != callArity)
            throw new XPathException("XPTY0004",
                "Expected base expression of a function call to evaluate to a sequence of single function item");

        return functionItem;
    }
}