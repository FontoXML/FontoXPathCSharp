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
        return PerformFunctionalEvaluation(
            dynamicContext, 
            executionParameters,
            this._childExpressions.Select(
                (expr) => (innerDynamicContext) =>
        expr.evaluate(innerDynamicContext, executionParameters)
            ));
    }

    public abstract ISequence PerformFunctionalEvaluation(DynamicContext? dynamicContext,
        ExecutionParameters? executionParameters, Func<DynamicContext, ISequence>[] sequenceCallbacks);

    public override void PerformStaticEvaluation(StaticContext staticContext)
    {
        base.PerformStaticEvaluation(staticContext);
        // TODO: this.DetermineUpdatingness();
    }
}