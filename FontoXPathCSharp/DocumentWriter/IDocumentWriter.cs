namespace FontoXPathCSharp.DocumentWriter;

public interface IDocumentWriter<TNode> where TNode : notnull
{
    void InsertBefore(TNode parent, TNode newNode, TNode? referenceNode);

    void RemoveAttributeNs(TNode node, string namespaceName, string name);

    void RemoveChild(TNode parent, TNode child);

    void SetAttributeNs(TNode node, string namespaceName, string name, string value);

    void SetData(TNode node, string data);
}