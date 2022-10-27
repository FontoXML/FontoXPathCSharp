using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public abstract class AbstractTestExpression<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    protected AbstractTestExpression(Specificity specificity) :
        base(specificity, Array.Empty<AbstractExpression<TNode>>(), 
            new OptimizationOptions(false))
    {
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        return EvaluateToBoolean(dynamicContext, dynamicContext?.ContextItem!, executionParameters)
            ? SequenceFactory.SingletonTrueSequence
            : SequenceFactory.SingletonFalseSequence;
    }

    protected internal abstract bool EvaluateToBoolean(
        DynamicContext? dynamicContext,
        AbstractValue value,
        ExecutionParameters<TNode>? executionParameters
    );
}