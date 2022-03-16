namespace FontoXPathCSharp.Types.Node;

public abstract class Attr : Node
{
    string localName;
    string name;
    string? namespaceURI;
    string nodeName;
    string? prefix;
    string value;
}