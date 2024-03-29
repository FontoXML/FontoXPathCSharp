using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class ChildAxis<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly AbstractTestExpression<TNode> _childExpression;
    private readonly string? _filterBucket;

    public ChildAxis(AbstractTestExpression<TNode> childExpression, string? filterBucket) : base(
        childExpression.Specificity,
        new AbstractExpression<TNode>[] { childExpression },
        new OptimizationOptions(
            false,
            true,
            ResultOrdering.Sorted,
            true)
    )
    {
        _childExpression = childExpression;
        _filterBucket = BucketUtils.IntersectBuckets(filterBucket, childExpression.GetBucket());
    }

    public override string ToString()
    {
        return $"ChildAxis[ {_childExpression} ]";
    }


    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var domFacade = executionParameters!.DomFacade;
        var contextNode = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext?.ContextItem);
        var nodeType = domFacade.GetNodeType(contextNode);

        if (nodeType != NodeType.Element && nodeType != NodeType.Document) return SequenceFactory.CreateEmpty();

        var node = default(TNode);
        var isDone = false;

        return SequenceFactory.CreateFromIterator(_ =>
        {
            while (!isDone)
            {
                if (node == null)
                {
                    node = domFacade.GetFirstChild(contextNode, _filterBucket);
                    if (node == null)
                    {
                        isDone = true;
                        continue;
                    }

                    return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(node, domFacade));
                }

                node = domFacade.GetNextSibling(node, _filterBucket);
                if (node == null)
                {
                    isDone = true;
                    continue;
                }

                return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(node, domFacade));
            }

            return IteratorResult<AbstractValue>.Done();
        }).Filter((item, _, _) => _childExpression.EvaluateToBoolean(
            dynamicContext,
            item,
            executionParameters)
        );
    }
}