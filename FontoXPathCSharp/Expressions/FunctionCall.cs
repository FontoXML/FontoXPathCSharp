using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class FunctionCall : PossiblyUpdatingExpression
{
    private readonly AbstractExpression[] _argumentExpressions;
    private readonly int _callArity;
    private readonly AbstractExpression _functionReferenceExpression;
    private FunctionValue<ISequence>? _functionReference;
    private StaticContext? _staticContext;

    public FunctionCall(AbstractExpression functionReferenceExpression, AbstractExpression[] args) : base(
        new[] { functionReferenceExpression }.Concat(args).ToArray(),
        new OptimizationOptions(false))
    {
        _argumentExpressions = args;
        _callArity = args.Length;
        _functionReferenceExpression = functionReferenceExpression;
        _staticContext = null;
    }

    public override ISequence PerformFunctionalEvaluation(DynamicContext? dynamicContext,
        ExecutionParameters? executionParameters)
    {
        if (_functionReference != null)
            return _functionReference.Value(dynamicContext, executionParameters, null,
                _argumentExpressions.Select(x => x.Evaluate(dynamicContext, executionParameters)).ToArray());

        // TODO: perform other evaluation
        throw new NotImplementedException();
    }

    public override void PerformStaticEvaluation(StaticContext staticContext)
    {
        _staticContext = staticContext.Clone();

        base.PerformStaticEvaluation(staticContext);

        if (!_functionReferenceExpression.CanBeStaticallyEvaluated) return;

        var functionRefSequence = _functionReferenceExpression.EvaluateMaybeStatically(null, null);
        if (!functionRefSequence.IsSingleton()) throw new XPathException("XPTY0004");

        _functionReference = ValidateFunctionItem<ISequence>(functionRefSequence.First()!, _callArity);

        // TODO: check if function reference is updating
    }

    private static FunctionValue<T> ValidateFunctionItem<T>(AbstractValue item, int callArity)
    {
        var functionItem = item.GetAs<FunctionValue<T>>(ValueType.Function);

        if (functionItem == null) throw new XPathException("Expected base expression to evaluate to a function item");

        if (functionItem.GetArity() != callArity) throw new XPathException("XPTY0004");

        return functionItem;
    }
}
