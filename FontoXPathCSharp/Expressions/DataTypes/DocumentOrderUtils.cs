using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes;

public class DocumentOrderUtils<TNode> where TNode : notnull
{
    private static List<T> Merge<T>(List<T> leftArray, List<T> rightArray, Comparison<T> comparer)
    {
        var sorted = new List<T>();

        while (leftArray.Count > 0 && rightArray.Count > 0)
            if (comparer(leftArray[0], rightArray[0]) < 0)
            {
                sorted.Add(leftArray[0]);
                leftArray.RemoveAt(0);
            }
            else
            {
                sorted.Add(rightArray[0]);
                rightArray.RemoveAt(0);
            }

        return sorted.Concat(leftArray.Concat(rightArray)).ToList();
    }

    public static List<T> MergeSort<T>(List<T> array, Comparison<T> comparer)
    {
        if (array.Count <= 1) return array;

        var mid = (int)Math.Floor(array.Count / 2.0);
        var left = MergeSort(array.Take(mid).ToList(), comparer);
        var right = MergeSort(array.Skip(mid).ToList(), comparer);

        return Merge(left, right, comparer);
    }

    public static List<NodeValue<TNode>> SortNodeValues(DomFacade<TNode> domFacade, List<NodeValue<TNode>> nodeValues)
    {
        return MergeSort(nodeValues, (node1, node2) => CompareNodePositionsWithTieBreaker(domFacade, node1, node2))
            .DistinctBy(n => n.Value)
            .ToList();
        // nodeValues.Sort((node1, node2) => CompareNodePositionsWithTieBreaker(domFacade, node1, node2));
        // return nodeValues.DistinctBy(n => n.Value).ToList();
    }

    private static int CompareNodePositionsWithTieBreaker(DomFacade<TNode> domFacade, NodeValue<TNode> node1,
        NodeValue<TNode> node2)
    {
        var isNode1SubtypeOfAttribute = node1.GetValueType().IsSubtypeOf(ValueType.Attribute);
        var isNode2SubtypeOfAttribute = node2.GetValueType().IsSubtypeOf(ValueType.Attribute);

        TNode? value1;
        TNode? value2;

        if (isNode1SubtypeOfAttribute && !isNode2SubtypeOfAttribute)
        {
            value1 = domFacade.GetParentNode(node1.Value);
            value2 = domFacade.GetParentNode(node2.Value);
            if (value1 != null && value1.Equals(value2))
                // Same element, so A
                return 1;
        }
        else if (isNode2SubtypeOfAttribute && !isNode1SubtypeOfAttribute)
        {
            value1 = node1.Value;
            value2 = domFacade.GetParentNode(node2.Value);
            if (value2 != null && value1.Equals(value2))
                // Same element, so B before A
                return -1;
        }
        else if (isNode1SubtypeOfAttribute && isNode2SubtypeOfAttribute)
        {
            var node1Parent = domFacade.GetParentNode(node1.Value);
            var node2Parent = domFacade.GetParentNode(node2.Value);
            if ((node1Parent != null && node1Parent.Equals(node2Parent)) ||
                (node1Parent == null && node2Parent == null))
                // Sort on attributes name
                return string.Compare(
                    domFacade.GetLocalName(node1.Value),
                    domFacade.GetLocalName(node2.Value),
                    StringComparison.Ordinal
                );

            value1 = node1Parent;
            value2 = node2Parent;
        }
        else
        {
            value1 = node1.Value;
            value2 = node2.Value;
        }

        return CompareElements(domFacade, value1!, value2!);
    }

    private static int CompareElements(DomFacade<TNode> domFacade, TNode nodeA, TNode nodeB)
    {
        // Comparing normal nodes. Can be optimized by disregarding pointers for ancestors

        var tieBreakerArr = domFacade.OrderOfDetachedNodes;

        var actualNodeA = nodeA;
        var actualNodeB = nodeB;

        if (actualNodeA.Equals(actualNodeB)) return 0;

        var actualAncestorsA = FindAllAncestors(domFacade, actualNodeA);
        var actualAncestorsB = FindAllAncestors(domFacade, actualNodeB);

        if (!actualAncestorsA[0].Equals(actualAncestorsB[0]))
        {
            var topAncestorA = actualAncestorsA[0];
            var topAncestorB = actualAncestorsB[0];
            // Separate trees, use earlier determined tie breakers
            var index1 = tieBreakerArr.FindIndex(e => e.Equals(topAncestorA));
            var index2 = tieBreakerArr.FindIndex(e => e.Equals(topAncestorB));
            if (index1 == -1)
            {
                tieBreakerArr.Add(topAncestorA);
                index1 = tieBreakerArr.Count;
            }

            if (index2 == -1)
            {
                tieBreakerArr.Add(actualNodeB);
                index2 = tieBreakerArr.Count;
            }

            return index1 - index2;
        }

        var y = 1;
        for (var z = Math.Min(actualAncestorsA.Count, actualAncestorsB.Count); y < z; ++y)
            if (!actualAncestorsA[y].Equals(actualAncestorsB[y]))
                break;

        // All nodes under a node are higher in document order than said node
        if (y >= actualAncestorsA.Count) return -1;

        // All nodes under a node are higher in document order than said node
        if (y >= actualAncestorsB.Count) return 1;

        var actualAncestorA = actualAncestorsA[y];
        var actualAncestorB = actualAncestorsB[y];

        // Compare positions under the common ancestor
        var parentNode = actualAncestorsB[y - 1];
        var childNodes = domFacade.GetChildNodes(parentNode);
        foreach (var childNode in childNodes)
        {
            if (Equals(childNode, actualAncestorA)) return -1;
            if (Equals(childNode, actualAncestorB)) return 1;
        }

        return 1;
    }

    private static List<TNode> FindAllAncestors(IDomFacade<TNode> domFacade, TNode node)
    {
        var ancestors = new List<TNode>();
        for (var ancestor = node; ancestor != null; ancestor = domFacade.GetParentNode(ancestor))
            ancestors.Insert(0, ancestor);

        return ancestors;
    }

    public static int CompareNodePositions(DomFacade<TNode> domFacade, NodeValue<TNode> node1, NodeValue<TNode> node2)
    {
        return CompareNodePositionsWithTieBreaker(
            domFacade,
            node1,
            node2
        );
    }
}