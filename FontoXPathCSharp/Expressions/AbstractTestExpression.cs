using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public abstract class AbstractTestAbstractExpression : AbstractExpression
{
    public override ISequence Evaluate(DynamicContext dynamicContext, ExecutionParameters executionParameters)
    {
        return new SingletonSequence(new BooleanValue(EvaluateToBoolean(dynamicContext, dynamicContext.ContextItem, executionParameters)));
    }

    protected internal abstract bool EvaluateToBoolean(DynamicContext dynamicContext,
        AbstractValue value,
        ExecutionParameters executionParameters);
}