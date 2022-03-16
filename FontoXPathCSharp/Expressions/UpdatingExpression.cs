using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class UpdatingExpressionResult
{
}

public abstract class UpdatingExpression : AbstractExpression
{
    protected UpdatingExpression(AbstractExpression[] childExpressions, OptimizationOptions optimizationOptions) : base(
        childExpressions, optimizationOptions)
    {
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        throw new XPathException("XUST0001");
    }

    public abstract IIterator<UpdatingExpressionResult> EvaluateWithUpdateList(DynamicContext? dynamicContext,
        ExecutionParameters executionParameters);
}