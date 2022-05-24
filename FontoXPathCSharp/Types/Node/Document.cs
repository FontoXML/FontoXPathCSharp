using System.Xml;

namespace FontoXPathCSharp.Types.Node;

public abstract class Document : Node
{
    public Document(string namespaceUri, string qualifiedNameStr, XmlNodeType documentType)
    {
    }

    public abstract Attr CreateAttributeNS(string namespaceUri, string name);
    public abstract CDATASection CreateCDATASection(string contents);
    public abstract Comment CreateComment(string data);
    public abstract Element CreateElementNS(string namespaceUri, string qualifiedName);
    public abstract ProcessingInstruction CreateProcessingInstruction(string target, string data);
    public abstract ProcessingInstruction CreateTextNode(string data);
}