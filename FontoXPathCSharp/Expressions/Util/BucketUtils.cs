using FontoXPathCSharp.DomFacade;

namespace FontoXPathCSharp.Expressions.Util;

public class BucketUtils
{
    private static readonly IReadOnlyDictionary<string, string[]> SubBucketsByBucket = new Dictionary<string, string[]>
    {
        { "type-1-or-type-2", new[] { "name", "type-1", "type-2" } },
        { "type-1", new[] { "name" } },
        { "type-2", new[] { "name" } }
    };

    // TODO: Think of a better way to do this rather than with string compares.
    // Ideas: Creating some subcategories of bucket classes for the parameterizable ones,
    // and the standard ones get done with enums.
    public static int GetBucketTypeId(NodeType nodeType)
    {
        return nodeType switch
        {
            NodeType.Element => 1,
            NodeType.Attribute => 2,
            NodeType.Text => 3,
            NodeType.CData => 4,
            NodeType.ProcessingInstruction => 7,
            NodeType.Comment => 8,
            NodeType.Document => 9,
            NodeType.DocumentType => 10,
            NodeType.DocumentFragment => 11,
            _ => throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null)
        };
    }

    public static string[] CreateBuckets(NodeType nodeType, string? localName)
    {
        var buckets = new List<string>();

        if (nodeType == NodeType.Attribute || nodeType == NodeType.Element) buckets.Add("type-1-or-type-2");

        buckets.Add($"type-{GetBucketTypeId(nodeType)}");

        if (localName != null) buckets.Add($"name-${localName}");

        return buckets.ToArray();
    }

    public static string[] GetBucketsForNode<TNode>(TNode node, IDomFacade<TNode> domFacade)
    {
        var nodeType = domFacade.GetNodeType(node);
        string? localName = null;

        if (nodeType == NodeType.Attribute || nodeType == NodeType.Element) localName = domFacade.GetLocalName(node);

        return CreateBuckets(nodeType, localName);
    }

    public static string? IntersectBuckets(string? bucket1, string? bucket2)
    {
        // null bucket applies to everything
        if (bucket1 == null) return bucket2;
        if (bucket2 == null) return bucket1;
        // Same bucket is same
        if (bucket1 == bucket2) return bucket1;
        // Find the more specific one, given that the buckets are not equal
        var type1 = bucket1.StartsWith("name-") ? "name" : bucket1;
        var type2 = bucket2.StartsWith("name-") ? "name" : bucket2;
        if (SubBucketsByBucket.ContainsKey(type1) &&
            SubBucketsByBucket[type1].Contains(type2)) // bucket 2 is more specific
            return bucket2;
        if (SubBucketsByBucket.ContainsKey(type2) &&
            SubBucketsByBucket[type2].Contains(type1)) // bucket 1 is more specific
            return bucket1;

        // Expression will never match any nodes
        return "empty";
    }
}