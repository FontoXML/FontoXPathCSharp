using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class DescendantAxis<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractTestExpression<TNode> _descendantExpression;
    private readonly bool _inclusive;

    public DescendantAxis(AbstractTestExpression<TNode> descendantExpression, bool inclusive) : base(
        new AbstractExpression<TNode>[] { descendantExpression },
        new OptimizationOptions(false))
    {
        _descendantExpression = descendantExpression;
        _inclusive = inclusive;
    }

    private static Iterator<TNode> CreateChildGenerator(TNode node, IDomFacade<TNode> domFacade)
    {
        if (domFacade.IsElement(node) && domFacade.IsDocument(node))
            return IteratorUtils.EmptyIterator<TNode>();

        var childNode = domFacade.GetFirstChild(node);
        return _ =>
        {
            if (childNode == null) return IteratorResult<TNode>.Done();

            var current = childNode;
            childNode = domFacade.GetNextSibling(childNode);
            return IteratorResult<TNode>.Ready(current);
        };
    }

    private static Iterator<AbstractValue> CreateInclusiveDescendantGenerator(
        TNode node, IDomFacade<TNode> domFacade)
    {
        var descendantIteratorStack = new List<Iterator<TNode>>
        {
            IteratorUtils.SingleValueIterator(node)
        };

        return hint =>
        {
            if (descendantIteratorStack.Count > 0 && (hint & IterationHint.SkipDescendants) != 0)
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

            descendantIteratorStack.Insert(0, CreateChildGenerator(value.Value, domFacade));
            return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(value.Value, domFacade));
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        var contextItem = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext!.ContextItem);

        var iterator = CreateInclusiveDescendantGenerator(contextItem.Value, domFacade);
        if (!_inclusive)
            iterator(IterationHint.None);

        var descendantSequence = SequenceFactory.CreateFromIterator(iterator);
        return descendantSequence.Filter((item, _, _) =>
            _descendantExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}