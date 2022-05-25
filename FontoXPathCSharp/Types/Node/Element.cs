namespace FontoXPathCSharp.Types.Node;

public abstract class Element : Node
{
    // TODO: remove this pragma call when appropriate
#pragma warning disable 0169
    private string? _localName;
    private string? _namespaceUri;
    private string? _nodeName;
    private string? _prefix;
#pragma warning restore 0169
}
