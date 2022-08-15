using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public abstract class UpdatingExpression<TNode> : AbstractExpression<TNode>
{
    protected UpdatingExpression(AbstractExpression<TNode>[] childExpressions,
        OptimizationOptions optimizationOptions) : base(
        childExpressions, optimizationOptions)
    {
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        throw new XPathException("XUST0001", "Can not execute an updating expression in a non-updating context.");
    }
}