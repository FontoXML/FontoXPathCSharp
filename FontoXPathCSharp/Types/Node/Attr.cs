namespace FontoXPathCSharp.Types.Node;

public abstract class Attr : Node
{
    private string _localName;
    private string _name;
    private string? _namespaceUri;
    private string _nodeName;
    private string? _prefix;
    private string _value;
}