namespace FontoXPathCSharp.Types.Node;

public abstract class Element : Node
{
    string localName;
    string? namespaceURI;
    string nodeName;
    string? prefix;
}