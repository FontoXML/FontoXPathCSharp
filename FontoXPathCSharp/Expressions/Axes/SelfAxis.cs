using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions.Axes;

public class SelfAxis : AbstractExpression
{
    private readonly AbstractTestExpression _selector;

    public SelfAxis(AbstractTestExpression selector) : base(new AbstractExpression[] {selector},
        new OptimizationOptions(false))
    {
        _selector = selector;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var isMatch = _selector.EvaluateToBoolean(dynamicContext, dynamicContext?.ContextItem!, executionParameters);
        return isMatch ? SequenceFactory.CreateFromValue(dynamicContext?.ContextItem!) : SequenceFactory.CreateEmpty();
    }
}