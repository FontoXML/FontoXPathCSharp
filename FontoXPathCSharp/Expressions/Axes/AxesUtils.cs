using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public static class AxesUtils<TNode>
{
    public static Iterator<AbstractValue> CreateDescendantIterator(IDomFacade<TNode> domFacade, TNode node,
        bool returnInReverse)
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
                currentNode = FindDeepestLastDescendant(node, domFacade);
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
                : domFacade.GetPreviousSibling(currentNode);

            if (previousSibling != null)
            {
                currentNode = FindDeepestLastDescendant(previousSibling, domFacade);
                return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(currentNode, domFacade));
            }

            currentNode = nodeType == NodeType.Document ? default : domFacade.GetParentNode(currentNode);
            if (currentNode != null && currentNode.Equals(node))
            {
                isDone = true;
                return IteratorResult<AbstractValue>.Done();
            }

            // TODO: null check somehow
            return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(currentNode, domFacade));
        };
    }

    private static TNode FindDeepestLastDescendant(TNode node, IDomFacade<TNode> domFacade)
    {
        if (domFacade.IsElement(node) && domFacade.IsDocument(node))
            return node;

        var parentNode = node;
        var childNode = domFacade.GetLastChild(node);
        while (childNode != null)
        {
            if (!domFacade.IsElement(childNode))
                return childNode;

            parentNode = childNode;
            childNode = domFacade.GetLastChild(parentNode);
        }

        return parentNode;
    }
}