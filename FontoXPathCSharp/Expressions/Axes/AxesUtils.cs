using System.Xml;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public static class AxesUtils
{
    public static Iterator<AbstractValue> CreateDescendantIterator(XmlNode node, bool returnInReverse)
    {
        if (!returnInReverse)
            return IteratorUtils.EmptyIterator<AbstractValue>();

        var currentNode = node;
        var isDone = false;
        return _ =>
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
}