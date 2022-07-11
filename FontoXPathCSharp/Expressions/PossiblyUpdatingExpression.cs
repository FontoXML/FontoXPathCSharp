using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public abstract class PossiblyUpdatingExpression : UpdatingExpression
{
    public delegate ISequence SequenceCallback(DynamicContext context);

    protected PossiblyUpdatingExpression(AbstractExpression[] childExpressions, OptimizationOptions optimizationOptions)
        : base(childExpressions, optimizationOptions)
    {
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        return PerformFunctionalEvaluation(
            dynamicContext,
            executionParameters,
            _childExpressions.Select<AbstractExpression, SequenceCallback>(
                expr => innerDynamicContext =>
                    expr.Evaluate(innerDynamicContext, executionParameters)).ToArray());
    }

    public abstract ISequence PerformFunctionalEvaluation(DynamicContext? dynamicContext,
        ExecutionParameters? executionParameters, SequenceCallback[] sequenceCallbacks);

    public override void PerformStaticEvaluation(StaticContext staticContext)
    {
        base.PerformStaticEvaluation(staticContext);
        // TODO: this.DetermineUpdatingness();
    }
}