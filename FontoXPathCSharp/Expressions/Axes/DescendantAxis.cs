using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class DescendantAxis<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly string? _descendantBucket;
    private readonly AbstractTestExpression<TNode> _descendantExpression;
    private readonly bool _inclusive;

    public DescendantAxis(
        AbstractTestExpression<TNode> descendantExpression,
        bool inclusive = false)
        : base(descendantExpression.Specificity,
            new AbstractExpression<TNode>[] { descendantExpression },
            new OptimizationOptions(
                false,
                false,
                ResultOrdering.Sorted,
                true)
        )
    {
        _descendantExpression = descendantExpression;
        _inclusive = inclusive;

        var testBucket = descendantExpression.GetBucket();
        var onlyElementDescendants = (testBucket != null &&
                                      (testBucket.StartsWith(BucketConstants.NamePrefix) ||
                                       testBucket == BucketConstants.Type1)) ||
                                     testBucket == BucketConstants.Type1OrType2;
        _descendantBucket = onlyElementDescendants ? BucketConstants.Type1 : null;
    }

    private static Iterator<TNode> CreateChildGenerator(TNode node, IDomFacade<TNode> domFacade, string? bucket)
    {
        var nodeType = domFacade.GetNodeType(node);
        if (nodeType != NodeType.Element && nodeType != NodeType.Document)
            return IteratorUtils.EmptyIterator<TNode>();


        var childNode = domFacade.GetFirstChild(node, bucket);
        return _ =>
        {
            if (childNode == null) return IteratorResult<TNode>.Done();

            var current = childNode;
            childNode = domFacade.GetNextSibling(childNode, bucket);
            return IteratorResult<TNode>.Ready(current);
        };
    }

    private static Iterator<AbstractValue> CreateInclusiveDescendantGenerator(
        TNode node,
        IDomFacade<TNode> domFacade,
        string? bucket)
    {
        var descendantIteratorStack = new List<Iterator<TNode>> { IteratorUtils.SingleValueIterator(node) };

        return hint =>
        {
            if (descendantIteratorStack.Count > 0 && (hint & IterationHint.SkipDescendants) != 0)
                // The next iterator on the stack will iterate over the last value's children, skip
                // it to skip the entire subtree
                descendantIteratorStack.RemoveAt(0);

            if (descendantIteratorStack.Count == 0)
                return IteratorResult<AbstractValue>.Done();

            var value = descendantIteratorStack.First()(IterationHint.None);
            while (value.IsDone)
            {
                descendantIteratorStack.RemoveAt(0);
                if (descendantIteratorStack.Count == 0)
                    return IteratorResult<AbstractValue>.Done();

                value = descendantIteratorStack.First()(IterationHint.None);
            }

            descendantIteratorStack.Insert(0, CreateChildGenerator(value.Value!, domFacade, bucket));
            return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(value.Value!, domFacade));
        };
    }

    public override ISequence Evaluate(
        DynamicContext? dynamicContext,
        ExecutionParameters<TNode>? executionParameters)
    {
        var domFacade = executionParameters!.DomFacade;
        var contextItem = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext!.ContextItem);

        var iterator = CreateInclusiveDescendantGenerator(
            contextItem.Value,
            domFacade,
            _descendantBucket
        );
        if (!_inclusive) iterator(IterationHint.None);

        var descendantSequence = SequenceFactory.CreateFromIterator(iterator);
        return descendantSequence.Filter((item, _, _) =>
            _descendantExpression.EvaluateToBoolean(
                dynamicContext,
                item,
                executionParameters)
        );
    }
}