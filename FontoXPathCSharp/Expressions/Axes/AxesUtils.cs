using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public static class AxesUtils<TNode>
{
    public static Iterator<AbstractValue> CreateDescendantIterator(
        IDomFacade<TNode> domFacade, 
        TNode node,
        bool returnInReverse,
        string? bucket)
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
                currentNode = FindDeepestLastDescendant(node, domFacade, bucket);
                if (currentNode.Equals(node))
                {
                    isDone = true;
                    return IteratorResult<AbstractValue>.Done();
                }

                return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(currentNode, domFacade));
            }

            var nodeType = domFacade.GetNodeType(currentNode);
            var previousSibling = nodeType is NodeType.Document or NodeType.Attribute
                ? default
                : domFacade.GetPreviousSibling(currentNode, bucket);

            if (previousSibling != null)
            {
                currentNode = FindDeepestLastDescendant(previousSibling, domFacade, bucket);
                return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(currentNode, domFacade));
            }

            currentNode = nodeType == NodeType.Document ? default : domFacade.GetParentNode(currentNode, bucket);
            if (currentNode != null && currentNode.Equals(node))
            {
                isDone = true;
                return IteratorResult<AbstractValue>.Done();
            }

            // TODO: null check somehow
            return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(currentNode, domFacade));
        };
    }

    private static TNode FindDeepestLastDescendant(TNode node, IDomFacade<TNode> domFacade, string? bucket)
    {
        if (domFacade.IsElement(node) && domFacade.IsDocument(node))
            return node;

        var parentNode = node;
        var childNode = domFacade.GetLastChild(node, bucket);
        while (childNode != null)
        {
            if (!domFacade.IsElement(childNode))
                return childNode;

            parentNode = childNode;
            childNode = domFacade.GetLastChild(parentNode, bucket);
        }

        return parentNode;
    }
}