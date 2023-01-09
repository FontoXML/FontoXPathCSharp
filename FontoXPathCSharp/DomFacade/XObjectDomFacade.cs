using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp.Expressions.Util;

namespace FontoXPathCSharp.DomFacade;

public class XObjectDomFacade : IDomFacade<XObject>
{
    public IEnumerable<XObject> GetAllAttributes(XObject node, string? bucket = null)
    {
        if (node.NodeType != XmlNodeType.Element) return Array.Empty<XObject>();
        var attrs = (node as XElement)?.Attributes() ?? Array.Empty<XAttribute>();
        return bucket == null
            ? attrs
            : attrs.Where(attr => BucketUtils.GetBucketsForNode(attr, this).Contains(bucket));
    }

    public string? GetAttribute(XObject node, string? attributeName)
    {
        if (node.NodeType != XmlNodeType.Element) return null;

        return (node as XElement)?.Attribute(attributeName ?? string.Empty)?.Value;
    }

    public IEnumerable<XObject> GetChildNodes(XObject node, string? bucket = null)
    {
        var childNodes = (node as XContainer)?.Nodes() ?? Array.Empty<XNode>();
        return bucket == null
            ? childNodes
            : childNodes.Where(child => BucketUtils.GetBucketsForNode(child, this).Contains(bucket));
    }

    public string GetData(XObject node)
    {
        return node.NodeType switch
        {
            XmlNodeType.Attribute => (node as XAttribute)?.Value,
            XmlNodeType.ProcessingInstruction => (node as XProcessingInstruction)?.Data,
            XmlNodeType.Comment => (node as XComment)?.Value,
            XmlNodeType.Text or XmlNodeType.CDATA => (node as XText)?.Value,
            _ => throw new Exception($"Unexpected node type in XObjectDomFacade.GetData: {node.NodeType}")
        } ?? string.Empty;
    }

    public XObject? GetFirstChild(XObject node, string? bucket = null)
    {
        for (var child = (node as XContainer)?.FirstNode; child != null; child = child.NextNode)
            if (bucket == null || BucketUtils.GetBucketsForNode(child, this).Contains(bucket))
                return child;
        return null;
    }

    public XObject? GetLastChild(XObject node, string? bucket = null)
    {
        for (var child = (node as XContainer)?.LastNode; child != null; child = child.PreviousNode)
            if (bucket == null || BucketUtils.GetBucketsForNode(child, this).Contains(bucket))
                return child;
        return null;
    }

    public XObject? GetNextSibling(XObject node, string? bucket = null)
    {
        for (var sibling = (node as XNode)?.NextNode; sibling != null; sibling = sibling.NextNode)
            if (bucket == null || BucketUtils.GetBucketsForNode(sibling, this).Contains(bucket))
                return sibling;
        return null;
    }

    public XObject? GetParentNode(XObject node, string? bucket = null)
    {
        var parentNode = node.Parent;
        if (parentNode == null)
        {
            return node.Document != null && node.Document.Nodes().Contains(node) ? node.Document : null;
        }
        return bucket == null || BucketUtils.GetBucketsForNode(parentNode, this).Contains(bucket)
            ? parentNode
            : null;
    }

    public XObject? GetPreviousSibling(XObject node, string? bucket = null)
    {
        for (var sibling = (node as XNode)?.PreviousNode; sibling != null; sibling = sibling.PreviousNode)
            if (bucket == null || BucketUtils.GetBucketsForNode(sibling, this).Contains(bucket))
                return sibling;
        return null;
    }

    public string GetLocalName(XObject node)
    {
        return node.NodeType switch
        {
            XmlNodeType.Element => (node as XElement)?.Name.LocalName,
            XmlNodeType.Attribute => (node as XAttribute)?.Name.LocalName,
            _ => throw new ArgumentOutOfRangeException()
        } ?? string.Empty;
    }

    public string GetNamespaceUri(XObject node)
    {
        return node.NodeType switch
        {
            XmlNodeType.Element => (node as XElement)?.Name.NamespaceName,
            XmlNodeType.Attribute => (node as XAttribute)?.Name.NamespaceName,
            _ => throw new ArgumentOutOfRangeException()
        } ?? string.Empty;
    }

    public string GetPrefix(XObject node)
    {
        var element = node as XElement;
        return element?.GetPrefixOfNamespace(element.Name.Namespace) ?? "";
    }

    public bool IsElement(XObject node)
    {
        return node.NodeType == XmlNodeType.Element;
    }

    public bool IsAttribute(XObject node)
    {
        return node.NodeType == XmlNodeType.Attribute;
    }

    public bool IsText(XObject node)
    {
        return node.NodeType == XmlNodeType.Text;
    }

    public bool IsProcessingInstruction(XObject node)
    {
        return node.NodeType == XmlNodeType.ProcessingInstruction;
    }

    public bool IsComment(XObject node)
    {
        return node.NodeType == XmlNodeType.Comment;
    }

    public bool IsDocument(XObject node)
    {
        return node.NodeType == XmlNodeType.Document;
    }

    public bool IsDocumentFragment(XObject node)
    {
        return node.NodeType == XmlNodeType.DocumentFragment;
    }

    public bool IsCharacterData(XObject node)
    {
        return node.NodeType == XmlNodeType.CDATA;
    }

    public NodeType GetNodeType(XObject node)
    {
        return node.NodeType switch
        {
            XmlNodeType.Element => NodeType.Element,
            XmlNodeType.Attribute => NodeType.Attribute,
            XmlNodeType.Text => NodeType.Text,
            XmlNodeType.CDATA => NodeType.CData,
            XmlNodeType.ProcessingInstruction => NodeType.ProcessingInstruction,
            XmlNodeType.Comment => NodeType.Comment,
            XmlNodeType.Document => NodeType.Document,
            XmlNodeType.DocumentType => NodeType.DocumentType,
            XmlNodeType.DocumentFragment => NodeType.DocumentFragment,
            _ => NodeType.OtherNode
        };
    }

    public XObject? GetDocumentElement(XObject node)
    {
        return (node as XDocument)?.Root;
    }

    public string GetNodeName(XObject node)
    {
        return node.NodeType switch
        {
            XmlNodeType.Element => ((XElement)node).Name.ToString(),
            XmlNodeType.Attribute => ((XAttribute)node).Name.ToString(),
            _ => ""
        };
    }

    public string? GetTarget(XObject node)
    {
        return (node as XProcessingInstruction)?.Target;
    }
}