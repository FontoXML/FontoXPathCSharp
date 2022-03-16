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
    protected AbstractExpression[] ChildExpressions;

    public AbstractExpression(AbstractExpression[] childExpressions, OptimizationOptions optimizationOptions)
    {
        ChildExpressions = childExpressions;
        CanBeStaticallyEvaluated = optimizationOptions.CanBeStaticallyEvaluated;
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

    public void PerformStaticEvaluation(StaticContext staticContext)
    {
        foreach (var expression in ChildExpressions)
        {
            expression.PerformStaticEvaluation(staticContext);
        }

        // TODO: make sure child expressions are not updating if we cannot be updating
    }
}