namespace FontoXPathCSharp.Types.Node;

public abstract class Element : Node
{
    protected Element(string? localName, string? namespaceUri, string? nodeName, string? prefix)
    {
        LocalName = localName;
        NamespaceUri = namespaceUri;
        NodeName = nodeName;
        Prefix = prefix;
    }

    public string? LocalName { get; }

    public string? NamespaceUri { get; }

    public string? NodeName { get; }

    public string? Prefix { get; }
}