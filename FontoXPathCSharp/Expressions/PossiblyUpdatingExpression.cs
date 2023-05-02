using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public abstract class PossiblyUpdatingExpression<TNode> : UpdatingExpression<TNode> where TNode : notnull
{
    public delegate ISequence SequenceCallback(DynamicContext context);

    protected PossiblyUpdatingExpression(
        Specificity specificity, 
        AbstractExpression<TNode>?[] childExpressions,
        OptimizationOptions optimizationOptions)
        : base(specificity, childExpressions, optimizationOptions)
    {
        IsUpdating = childExpressions.Any(
            childExpression => childExpression?.IsUpdating ?? false
        );
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        return PerformFunctionalEvaluation(
            dynamicContext,
            executionParameters!,
            ChildExpressions.Select<AbstractExpression<TNode>, SequenceCallback>(
                expr => innerDynamicContext =>
                    expr.Evaluate(innerDynamicContext, executionParameters)).ToArray());
    }

    public abstract ISequence PerformFunctionalEvaluation(DynamicContext? dynamicContext,
        ExecutionParameters<TNode> executionParameters, SequenceCallback[] sequenceCallbacks);

    // TODO: this.DetermineUpdatingness();
}