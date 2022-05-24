namespace FontoXPathCSharp.Types.Node;

public abstract class Element : Node
{
    private string localName;
    private string? namespaceURI;
    private string nodeName;
    private string? prefix;
}