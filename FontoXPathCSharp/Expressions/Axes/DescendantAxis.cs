using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class DescendantAxis : AbstractExpression
{
    private readonly AbstractTestExpression _descendantExpression;

    public DescendantAxis(AbstractTestExpression descendantExpression) : base(
        new AbstractExpression[] { descendantExpression },
        new OptimizationOptions(false))
    {
        _descendantExpression = descendantExpression;
    }

    private static Iterator<XmlNode> CreateChildGenerator(XmlNode node)
    {
        var nodeType = node.NodeType;
        if (nodeType != XmlNodeType.Element && nodeType != XmlNodeType.Document)
            return IteratorUtils.EmptyIterator<XmlNode>();

        var childNode = node.FirstChild;
        return _ =>
        {
            if (childNode == null) return IteratorResult<XmlNode>.Done();

            var current = childNode;
            childNode = childNode.NextSibling;
            return IteratorResult<XmlNode>.Ready(current);
        };
    }

    private static Iterator<AbstractValue> CreateInclusiveDescendantGenerator(
        XmlNode node)
    {
        var descendantIteratorStack = new List<Iterator<XmlNode>>
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

            descendantIteratorStack.Insert(0, CreateChildGenerator(value.Value));
            return IteratorResult<AbstractValue>.Ready(new NodeValue(value.Value));
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var contextItem = ContextNodeUtils.ValidateContextNode(dynamicContext!.ContextItem!);

        // TODO: add inclusive property
        var iterator = CreateInclusiveDescendantGenerator(contextItem.Value);
        iterator(IterationHint.None);
        var descendantSequence = SequenceFactory.CreateFromIterator(iterator);
        return descendantSequence.Filter((item, _, _) =>
            _descendantExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}