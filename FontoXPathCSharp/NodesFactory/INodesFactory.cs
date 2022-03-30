using FontoXPathCSharp.Types.Node;

namespace FontoXPathCSharp.NodesFactory;

public interface INodesFactory
{
    public Document CreateDocument();
}