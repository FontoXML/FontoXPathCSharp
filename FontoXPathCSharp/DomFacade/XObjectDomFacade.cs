using System.Xml;
using System.Xml.Linq;

namespace FontoXPathCSharp.DomFacade;

public class XObjectDomFacade : IDomFacade<XObject>
{
    public IEnumerable<XObject> GetAllAttributes(XObject node, string? bucket)
    {
        if (node.NodeType != XmlNodeType.Element) return Array.Empty<XObject>();

        return (node as XElement)?.Attributes() ?? Array.Empty<XAttribute>();
    }

    public string? GetAttribute(XObject node, string? attributeName)
    {
        if (node.NodeType != XmlNodeType.Element) return null;

        return (node as XElement)?.Attribute(attributeName ?? string.Empty)?.Value;
    }

    public IEnumerable<XObject> GetChildNodes(XObject node, string? bucket)
    {
        return (node as XContainer)?.Nodes() ?? Array.Empty<XNode>();
    }

    public string GetData(XObject node)
    {
        return node.NodeType switch
        {
            XmlNodeType.Attribute => (node as XAttribute)?.Value,
            XmlNodeType.ProcessingInstruction => (node as XProcessingInstruction)?.Data,
            XmlNodeType.Comment => (node as XComment)?.Value,
            XmlNodeType.Text or XmlNodeType.CDATA => (node as XText)?.Value,
            _ => throw new Exception($"Unexpected nodetype in XObjectDomFacade.GetData: {node.NodeType}")
        } ?? string.Empty;
    }

    public XObject? GetFirstChild(XObject node, string? bucket)
    {
        return (node as XContainer)?.FirstNode;
    }

    public XObject? GetLastChild(XObject node, string? bucket)
    {
        return (node as XContainer)?.LastNode;
    }

    public XObject? GetNextSibling(XObject node, string? bucket)
    {
        return (node as XNode)?.NextNode;
    }

    public XObject? GetParentNode(XObject node, string? bucket)
    {
        return node.Parent;
    }

    public XObject? GetPreviousSibling(XObject node, string? bucket)
    {
        return (node as XNode)?.PreviousNode;
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
        throw new NotImplementedException("XObjectDomFacade.GetPrefix not implemented yet");
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

    public string? LookupNamespaceUri(XObject node, string? prefix)
    {
        if (string.IsNullOrEmpty(prefix)) prefix = null;

        if (node is null or XDocumentType) return null;
        throw new NotImplementedException(
            $"XObjectDomFacade.LookupNamespaceUri: Not implemented yet for {node.NodeType}");
        return LookupNamespaceUri(GetParentNode(node, null), prefix);
    }
}