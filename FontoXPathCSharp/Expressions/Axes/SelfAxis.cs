using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions.Axes;

public class SelfAxis<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractTestExpression<TNode> _selector;

    public SelfAxis(AbstractTestExpression<TNode> selector) : base(new AbstractExpression<TNode>[] { selector },
        new OptimizationOptions(false))
    {
        _selector = selector;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var isMatch = _selector.EvaluateToBoolean(dynamicContext, dynamicContext?.ContextItem!, executionParameters);
        return isMatch ? SequenceFactory.CreateFromValue(dynamicContext?.ContextItem!) : SequenceFactory.CreateEmpty();
    }
}