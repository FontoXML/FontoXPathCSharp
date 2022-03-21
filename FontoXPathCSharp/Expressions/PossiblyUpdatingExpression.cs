using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public abstract class PossiblyUpdatingExpression : UpdatingExpression
{
    protected PossiblyUpdatingExpression(AbstractExpression[] childExpressions, OptimizationOptions optimizationOptions)
        : base(childExpressions, optimizationOptions)
    {
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        return PerformFunctionalEvaluation(dynamicContext, executionParameters);
    }

    public override IIterator<UpdatingExpressionResult> EvaluateWithUpdateList(DynamicContext? dynamicContext,
        ExecutionParameters executionParameters)
    {
        throw new NotImplementedException();
    }

    public abstract ISequence PerformFunctionalEvaluation(DynamicContext? dynamicContext,
        ExecutionParameters? executionParameters /*, SequenceCallbacks sequenceCallbacks TODO: add sequenceCallbacks */);

    public override void PerformStaticEvaluation(StaticContext staticContext)
    {
        base.PerformStaticEvaluation(staticContext);
        // TODO: this.DetermineUpdatingness();
    }
}