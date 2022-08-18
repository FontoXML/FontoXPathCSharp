using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class FollowingSiblingAxis<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractTestExpression<TNode> _siblingExpression;
    private readonly string? _filterBucket;

    public FollowingSiblingAxis(AbstractTestExpression<TNode> siblingExpression, string? filterBucket) : base(
        new AbstractExpression<TNode>[] { siblingExpression },
        new OptimizationOptions(false))
    {
        _siblingExpression = siblingExpression;
        _filterBucket = BucketUtils.IntersectBuckets(siblingExpression.GetBucket(), filterBucket);
    }

    private static Iterator<AbstractValue> CreateSiblingIterator(IDomFacade<TNode> domFacade, TNode? node, string? bucket)
    {
        return _ =>
        {
            node = domFacade.GetNextSibling(node, bucket);

            return node == null
                ? IteratorResult<AbstractValue>.Done()
                : IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(node, domFacade));
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        var contextItem = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext!.ContextItem!);

        return SequenceFactory.CreateFromIterator(CreateSiblingIterator(domFacade, contextItem.Value, _filterBucket))
            .Filter((item, _, _) =>
                _siblingExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}