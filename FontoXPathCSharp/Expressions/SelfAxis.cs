namespace FontoXPathCSharp.Expressions;

using System.Xml;
using Sequences;
using Value;

public class SelfAxis : AbstractExpression
{
    private readonly AbstractTestAbstractExpression _selector;

    public SelfAxis(AbstractTestAbstractExpression selector)
    {
        _selector = selector;
    }

    public override ISequence Evaluate(DynamicContext dynamicContext, ExecutionParameters executionParameters)
    {
        var isMatch = _selector.EvaluateToBoolean(dynamicContext, dynamicContext.ContextItem, executionParameters);
        return isMatch ? new SingletonSequence(dynamicContext.ContextItem) : new EmptySequence();
    }
}