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
    public readonly bool Subtree;
    public readonly bool Peer;

    public OptimizationOptions(
        bool canBeStaticallyEvaluated = false, 
        bool peer = false,
        ResultOrdering resultOrder = ResultOrdering.Unsorted, 
        bool subtree = false)
    {
        CanBeStaticallyEvaluated = canBeStaticallyEvaluated;
        ResultOrder = resultOrder;
        Subtree = subtree;
        Peer = peer;
    }
}

public abstract class AbstractExpression<TNode>
{
    protected readonly AbstractExpression<TNode>[] _childExpressions;
    public readonly bool CanBeStaticallyEvaluated;
    public readonly bool IsUpdating;
    public readonly ResultOrdering ExpectedResultOrder;
    public readonly bool Subtree;
    public readonly bool Peer;

    protected AbstractExpression(AbstractExpression<TNode>[] childExpressions, OptimizationOptions optimizationOptions)
    {
        _childExpressions = childExpressions;
        CanBeStaticallyEvaluated = optimizationOptions.CanBeStaticallyEvaluated;
        ExpectedResultOrder = optimizationOptions.ResultOrder;
        Subtree = optimizationOptions.Subtree;
        Peer = optimizationOptions.Peer;
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