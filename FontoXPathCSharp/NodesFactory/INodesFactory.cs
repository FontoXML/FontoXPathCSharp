namespace FontoXPathCSharp.NodesFactory;

public interface INodesFactory<TNode> : ISimpleNodesFactory<TNode> where TNode : notnull
{
    TNode CreateDocument();
}