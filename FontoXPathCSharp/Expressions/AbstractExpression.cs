using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public struct OptimizationOptions
{
    public readonly bool CanBeStaticallyEvaluated;

    public OptimizationOptions(bool canBeStaticallyEvaluated)
    {
        CanBeStaticallyEvaluated = canBeStaticallyEvaluated;
    }
}

public abstract class AbstractExpression
{
    public readonly bool CanBeStaticallyEvaluated;
    private readonly AbstractExpression[] _childExpressions;

    public bool IsUpdating;
    
    protected AbstractExpression(AbstractExpression[] childExpressions, OptimizationOptions optimizationOptions)
    {
        _childExpressions = childExpressions;
        CanBeStaticallyEvaluated = optimizationOptions.CanBeStaticallyEvaluated;
        IsUpdating = false;
    }

    public abstract ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters);

    public ISequence EvaluateMaybeStatically(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        if (dynamicContext?.ContextItem == null)
        {
            return Evaluate(dynamicContext, executionParameters);
        }

        if (CanBeStaticallyEvaluated)
        {
            // TODO: return EvaluateWithoutFocus(dynamicContext, executionParameters);
        }

        return Evaluate(dynamicContext, executionParameters);
    }

    public virtual void PerformStaticEvaluation(StaticContext staticContext)
    {
        foreach (var expression in _childExpressions)
        {
            expression.PerformStaticEvaluation(staticContext);
        }

        // TODO: make sure child expressions are not updating if we cannot be updating
    }
}