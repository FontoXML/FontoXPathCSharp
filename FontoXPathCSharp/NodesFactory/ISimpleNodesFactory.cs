namespace FontoXPathCSharp.NodesFactory;

public interface ISimpleNodesFactory<TNode> where TNode : notnull
{
    TNode CreateAttributeNs(string namespaceUri, string name);

    TNode CreateCDataSection(string contents);

    TNode CreateComment(string contents);

    TNode CreateElementNs(string namespaceUri, string name);

    TNode CreateProcessingInstruction(string target, string data);

    TNode CreateTextNode(string contents);
}