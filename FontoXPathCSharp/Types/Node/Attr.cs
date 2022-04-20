namespace FontoXPathCSharp.Types.Node;

public abstract class Attr : Node
{
    private string localName;
    private string name;
    private string? namespaceURI;
    private string nodeName;
    private string? prefix;
    private string value;
}