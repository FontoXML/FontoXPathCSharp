using System.Xml;

namespace FontoXPathCSharp.DomFacade;

public class XmlNodeDomFacade : IDomFacade<XmlNode>
{
    public IEnumerable<XmlNode> GetAllAttributes(XmlNode node, string? bucket)
    {
        if (node.NodeType != XmlNodeType.Element) return Array.Empty<XmlNode>();

        return node.Attributes != null ? node.Attributes.Cast<XmlAttribute>() : Array.Empty<XmlAttribute>();
    }

    public string? GetAttribute(XmlNode node, string? attributeName)
    {
        if (node.NodeType != XmlNodeType.Element) return null;

        return node.Attributes?[attributeName ?? string.Empty]?.Value;
    }

    public IEnumerable<XmlNode> GetChildNodes(XmlNode node, string? bucket)
    {
        return node.ChildNodes.Cast<XmlNode>();
    }

    public string GetData(XmlNode node)
    {
        return node.Value;
    }

    public XmlNode? GetFirstChild(XmlNode node, string? bucket)
    {
        return node.FirstChild;
    }

    public XmlNode? GetLastChild(XmlNode node, string? bucket)
    {
        return node.LastChild;
    }

    public XmlNode? GetNextSibling(XmlNode node, string? bucket)
    {
        return node.NextSibling;
    }

    public XmlNode? GetParentNode(XmlNode node, string? bucket)
    {
        return node.ParentNode;
    }

    public XmlNode? GetPreviousSibling(XmlNode node, string? bucket)
    {
        return node.PreviousSibling;
    }

    public string GetLocalName(XmlNode node)
    {
        return node.LocalName;
    }

    public string GetNamespaceUri(XmlNode node)
    {
        return node.NamespaceURI;
    }

    public string? GetPrefix(XmlNode nodeValue)
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

    public string? LookupNamespaceUri(XmlNode node, string? prefix)
    {
        if (string.IsNullOrEmpty(prefix)) prefix = null;


        Console.WriteLine($"XmlNodeDomFacade.LookupNamespaceUri: Not implemented yet for {node.NodeType}");
        return null;
        return LookupNamespaceUri(GetParentNode(node, null), prefix);
    }
}