using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

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
}