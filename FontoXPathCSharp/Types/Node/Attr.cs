namespace FontoXPathCSharp.Types.Node;

public class Attr : Node
{
    private string? _localName;
    private string? _name;
    private string? _namespaceUri;
    private string? _nodeName;
    private string? _prefix;
    private string? _value;

    public string? LocalName => _localName;

    public string? Name => _name;

    public string? NamespaceUri => _namespaceUri;

    public string? NodeName => _nodeName;

    public string? Prefix => _prefix;

    public string? Value => _value;

    public Attr(string? localName, string? name, string? namespaceUri, string? nodeName, string? prefix, string? value)
    {
        _localName = localName;
        _name = name;
        _namespaceUri = namespaceUri;
        _nodeName = nodeName;
        _prefix = prefix;
        _value = value;
    }
}