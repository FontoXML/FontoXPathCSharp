using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class SelfAxis : AbstractExpression
{
    private readonly AbstractTestExpression _selector;

    public SelfAxis(AbstractTestExpression selector) : base(new AbstractExpression[] {selector}, new OptimizationOptions(false))
    {
        _selector = selector;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var isMatch = _selector.EvaluateToBoolean(dynamicContext, dynamicContext?.ContextItem!, executionParameters);
        return isMatch ? new SingletonSequence(dynamicContext?.ContextItem!) : new EmptySequence();
    }
}