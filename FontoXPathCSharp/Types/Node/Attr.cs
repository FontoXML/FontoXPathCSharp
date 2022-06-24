namespace FontoXPathCSharp.Types.Node;

public class Attr : Node
{
    public Attr(string? localName, string? name, string? namespaceUri, string? nodeName, string? prefix, string? value)
    {
        LocalName = localName;
        Name = name;
        NamespaceUri = namespaceUri;
        NodeName = nodeName;
        Prefix = prefix;
        Value = value;
    }

    public string? LocalName { get; }

    public string? Name { get; }

    public string? NamespaceUri { get; }

    public string? NodeName { get; }

    public string? Prefix { get; }

    public string? Value { get; }
}