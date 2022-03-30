using FontoXPathCSharp.Types.Node;

namespace FontoXPathCSharp.DocumentWriter;

public interface IDocumentWriter
{
    void InsertBefore(Element parent, Node newNode, Node? referenceNode);

    void InsertBefore(Document parent, Node newNode, Node? referenceNode);

    void RemoveAttributeNS(Element node, string namespaceName, string name);

    void RemoveChild(Element parent, Node child);

    void RemoveChild(Document parent, Node child);

    void SetAttributeNS(Element node, string namespaceName, string name, string value);

    void SetData(Node node, string data);
}