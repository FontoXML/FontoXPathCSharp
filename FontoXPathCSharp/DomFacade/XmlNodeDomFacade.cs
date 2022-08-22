using System.Xml;
using FontoXPathCSharp.Expressions.Util;

namespace FontoXPathCSharp.DomFacade;

public class XmlNodeDomFacade : IDomFacade<XmlNode>
{
    public IEnumerable<XmlNode> GetAllAttributes(XmlNode node, string? bucket = null)
    {
        if (node.NodeType != XmlNodeType.Element) return Array.Empty<XmlNode>();
        var attrs = node.Attributes != null ? node.Attributes.Cast<XmlAttribute>() : Array.Empty<XmlAttribute>();
        return bucket == null
            ? attrs
            : attrs.Where(attr => BucketUtils.GetBucketsForNode(attr, this).Contains(bucket));
    }

    public string? GetAttribute(XmlNode node, string? attributeName)
    {
        return node.NodeType == XmlNodeType.Element
            ? node.Attributes?[attributeName ?? string.Empty]?.Value
            : null;
    }

    public IEnumerable<XmlNode> GetChildNodes(XmlNode node, string? bucket = null)
    {
        var childNodes = node.ChildNodes.Cast<XmlNode>();
        return bucket == null
            ? childNodes
            : childNodes.Where(child => BucketUtils.GetBucketsForNode(child, this).Contains(bucket));
    }

    public string GetData(XmlNode node)
    {
        return node.Value!;
    }

    public XmlNode? GetFirstChild(XmlNode node, string? bucket = null)
    {
        // Not sure if this does anything productive.
        for (var child = node.FirstChild; child != null; child = child.NextSibling)
            if (bucket == null || BucketUtils.GetBucketsForNode(child, this).Contains(bucket))
                return child;
        return null;
    }

    public XmlNode? GetLastChild(XmlNode node, string? bucket = null)
    {
        // Not sure if this does anything productive.
        for (var child = node.LastChild; child != null; child = child.PreviousSibling)
            if (bucket == null || BucketUtils.GetBucketsForNode(child, this).Contains(bucket))
                return child;
        return null;
    }

    public XmlNode? GetNextSibling(XmlNode node, string? bucket = null)
    {
        // Not sure if this does anything productive.
        for (var sibling = node.NextSibling; sibling != null; sibling = sibling.NextSibling)
            if (bucket == null || BucketUtils.GetBucketsForNode(sibling, this).Contains(bucket))
                return sibling;
        return null;
    }

    public XmlNode? GetParentNode(XmlNode node, string? bucket = null)
    {
        var parentNode = node.NodeType == XmlNodeType.Attribute
            ? ((XmlAttribute)node).OwnerElement
            : node.ParentNode;
        if (parentNode == null) return null;

        return bucket == null || BucketUtils.GetBucketsForNode(parentNode, this).Contains(bucket)
            ? parentNode
            : null;
    }

    public XmlNode? GetPreviousSibling(XmlNode node, string? bucket = null)
    {
        for (var sibling = node.PreviousSibling; sibling != null; sibling = sibling.PreviousSibling)
            if (bucket == null || BucketUtils.GetBucketsForNode(sibling, this).Contains(bucket))
                return sibling;

        return null;
    }

    public string GetLocalName(XmlNode node)
    {
        return node.LocalName;
    }

    public string GetNamespaceUri(XmlNode node)
    {
        return node.NamespaceURI;
    }

    public string GetPrefix(XmlNode nodeValue)
    {
        return nodeValue.Prefix;
    }

    public bool IsElement(XmlNode node)
    {
        return node.NodeType == XmlNodeType.Element;
    }

    public bool IsAttribute(XmlNode node)
    {
        return node.NodeType == XmlNodeType.Attribute;
    }

    public bool IsText(XmlNode node)
    {
        return node.NodeType == XmlNodeType.Text;
    }

    public bool IsProcessingInstruction(XmlNode node)
    {
        return node.NodeType == XmlNodeType.ProcessingInstruction;
    }

    public bool IsComment(XmlNode node)
    {
        return node.NodeType == XmlNodeType.Comment;
    }

    public bool IsDocument(XmlNode node)
    {
        return node.NodeType == XmlNodeType.Document;
    }

    public bool IsDocumentFragment(XmlNode node)
    {
        return node.NodeType == XmlNodeType.DocumentFragment;
    }

    public bool IsCharacterData(XmlNode node)
    {
        return node.NodeType == XmlNodeType.CDATA;
    }

    public NodeType GetNodeType(XmlNode node)
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

    public XmlNode? GetDocumentElement(XmlNode node)
    {
        return (node as XmlDocument)?.DocumentElement;
    }
}