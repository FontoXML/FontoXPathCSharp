using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class ContextItemExpression<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    public ContextItemExpression() : base(
        new Specificity(), Array.Empty<AbstractExpression<TNode>>(),
        new OptimizationOptions(false, resultOrder: ResultOrdering.Sorted))
    {
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        return dynamicContext?.ContextItem == null
            ? throw new XPathException(
                "XPDY0002",
                "context is absent, it needs to be present to use the \".\" operator"
            )
            : SequenceFactory.CreateFromValue(dynamicContext.ContextItem);
    }
}