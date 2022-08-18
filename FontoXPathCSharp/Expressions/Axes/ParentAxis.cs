using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class ParentAxis<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractTestExpression<TNode> _parentExpression;
    private readonly string? _filterBucket;

    public ParentAxis(AbstractTestExpression<TNode> parentExpression, string? filterBucket) : base(
        new AbstractExpression<TNode>[] { parentExpression },
        new OptimizationOptions(false))
    {
        _parentExpression = parentExpression;
        _filterBucket = BucketUtils.IntersectBuckets(filterBucket, parentExpression.GetBucket());
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        var contextNode = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext.ContextItem);
        var parentNode = domFacade.GetParentNode(contextNode.Value, _filterBucket);

        if (parentNode == null) return SequenceFactory.CreateEmpty();

        var parentNodeValue = new NodeValue<TNode>(parentNode, domFacade);
        var nodeIsMatch = _parentExpression.EvaluateToBoolean(
            dynamicContext,
            parentNodeValue,
            executionParameters
        );

        return !nodeIsMatch ? SequenceFactory.CreateEmpty() : SequenceFactory.CreateFromValue(parentNodeValue);
    }
}