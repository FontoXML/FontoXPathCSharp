using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public abstract class AbstractTestExpression<TNode> : AbstractExpression<TNode>
{
    protected AbstractTestExpression(Specificity specificity) :
        base(specificity, Array.Empty<AbstractExpression<TNode>>(), new OptimizationOptions(false))
    {
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        return SequenceFactory.CreateFromValue(
            new BooleanValue(EvaluateToBoolean(dynamicContext, dynamicContext?.ContextItem!, executionParameters)));
    }

    protected internal abstract bool EvaluateToBoolean(DynamicContext? dynamicContext,
        AbstractValue value,
        ExecutionParameters<TNode> executionParameters);
}