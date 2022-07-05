using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class FollowingAxis : AbstractExpression
{
    private readonly AbstractTestExpression _testExpression;

    public FollowingAxis(AbstractTestExpression testExpression) : base(new AbstractExpression[] {testExpression},
        new OptimizationOptions(false))
    {
        _testExpression = testExpression;
    }

    private static XmlNode FindDeepestLastDescendant(XmlNode node)
    {
        var nodeType = node.NodeType;
        if (nodeType != XmlNodeType.Element && nodeType != XmlNodeType.Document)
            return node;

        var parentNode = node;
        var childNode = node.LastChild;
        while (childNode != null)
        {
            if (childNode.NodeType != XmlNodeType.Element)
                return childNode;

            parentNode = childNode;
            childNode = parentNode.LastChild;
        }

        return parentNode;
    }

    private static Iterator<AbstractValue> CreateDescendantIterator(XmlNode node, bool returnInReverse)
    {
        if (returnInReverse)
        {
            var currentNode = node;
            var isDone = false;
            return hint =>
            {
                if (isDone)
                    return IteratorResult<AbstractValue>.Done();

                if (currentNode.Equals(node))
                {
                    currentNode = FindDeepestLastDescendant(node);
                    if (currentNode.Equals(node))
                    {
                        isDone = true;
                        return IteratorResult<AbstractValue>.Done();
                    }

                    return IteratorResult<AbstractValue>.Ready(new NodeValue(currentNode));
                }

                var nodeType = currentNode.NodeType;
                var previousSibling = nodeType is XmlNodeType.Document or XmlNodeType.Attribute
                    ? null
                    : currentNode.PreviousSibling;

                if (previousSibling != null)
                {
                    currentNode = FindDeepestLastDescendant(previousSibling);
                    return IteratorResult<AbstractValue>.Ready(new NodeValue(currentNode));
                }

                currentNode = nodeType == XmlNodeType.Document ? null : currentNode.ParentNode;
                if (currentNode != null && currentNode.Equals(node))
                {
                    isDone = true;
                    return IteratorResult<AbstractValue>.Done();
                }

                // TODO: null check somehow
                return IteratorResult<AbstractValue>.Ready(new NodeValue(currentNode));
            };
        }

        return IteratorUtils.EmptyIterator<AbstractValue>();
    }

    private static Iterator<AbstractValue> CreateFollowingGenerator(XmlNode node)
    {
        var nodeStack = new List<XmlNode>();

        for (var ancestorNode = node;
             ancestorNode != null && ancestorNode.NodeType != XmlNodeType.Document;
             ancestorNode = ancestorNode.ParentNode)
        {
            var previousSibling = ancestorNode.NextSibling;
            if (previousSibling != null)
                nodeStack.Add(previousSibling);
        }

        Iterator<AbstractValue>? nephewGenerator = null;
        return _ =>
        {
            while (nephewGenerator != null || nodeStack.Count > 0)
            {
                if (nephewGenerator == null)
                {
                    nephewGenerator = CreateDescendantIterator(nodeStack.First(), false);

                    var toReturn = IteratorResult<AbstractValue>.Ready(new NodeValue(nodeStack.First()));

                    var nextNode = nodeStack.First().NextSibling;
                    if (nextNode == null)
                        nodeStack.RemoveAt(0);
                    else
                        nodeStack[0] = nextNode;

                    return toReturn;
                }

                var nephew = nephewGenerator(IterationHint.None);
                if (!nephew.IsDone)
                    return nephew;

                nephewGenerator = null;
            }

            return IteratorResult<AbstractValue>.Done();
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var contextItem = ContextNodeUtils.ValidateContextNode(dynamicContext!.ContextItem!);

        return SequenceFactory.CreateFromIterator(CreateFollowingGenerator(contextItem.Value)).Filter((item, _, _) =>
            _testExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}