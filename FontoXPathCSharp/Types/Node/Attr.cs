namespace FontoXPathCSharp.Types.Node;

public abstract class Attr : Node
{
    // TODO: remove this pragma call when appropriate
#pragma warning disable 0169
    private string? _localName;
    private string? _name;
    private string? _namespaceUri;
    private string? _nodeName;
    private string? _prefix;
    private string? _value;
#pragma warning restore 0169
}
