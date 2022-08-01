using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public enum ResultOrdering
{
    Sorted,
    ReverseSorted,
    Unsorted
}

public struct OptimizationOptions
{
    public readonly bool CanBeStaticallyEvaluated;
    public readonly ResultOrdering ResultOrder;

    public OptimizationOptions(bool canBeStaticallyEvaluated, ResultOrdering resultOrder = ResultOrdering.Unsorted)
    {
        CanBeStaticallyEvaluated = canBeStaticallyEvaluated;
        ResultOrder = resultOrder;
    }
}

public abstract class AbstractExpression<TNode>
{
    protected readonly AbstractExpression<TNode>[] _childExpressions;
    public readonly bool CanBeStaticallyEvaluated;

    public readonly bool IsUpdating;

    public readonly ResultOrdering ResultOrder;

    protected AbstractExpression(AbstractExpression<TNode>[] childExpressions, OptimizationOptions optimizationOptions)
    {
        _childExpressions = childExpressions;
        CanBeStaticallyEvaluated = optimizationOptions.CanBeStaticallyEvaluated;
        ResultOrder = optimizationOptions.ResultOrder;
        IsUpdating = false;
    }

    public abstract ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters);

    public ISequence EvaluateMaybeStatically(DynamicContext? dynamicContext,
        ExecutionParameters<TNode> executionParameters)
    {
        if (dynamicContext?.ContextItem == null) return Evaluate(dynamicContext, executionParameters);

        if (CanBeStaticallyEvaluated)
        {
            // TODO: return EvaluateWithoutFocus(dynamicContext, executionParameters);
        }

        return Evaluate(dynamicContext, executionParameters);
    }

    public virtual void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        foreach (var expression in _childExpressions) expression.PerformStaticEvaluation(staticContext);
        // TODO: make sure child expressions are not updating if we cannot be updating
    }
}