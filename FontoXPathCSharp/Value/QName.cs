namespace FontoXPathCSharp.Value;

public class QName
{
    public readonly string LocalName;
    public readonly string? Prefix;
    public string? NamespaceUri;

    public QName(string localName, string? namespaceUri = null, string? prefix = null)
    {
        LocalName = localName;
        NamespaceUri = namespaceUri;
        Prefix = prefix;
    }

    public Ast GetAst(AstNodeName name)
    {
        var ast = new Ast(name)
        {
            TextContent = LocalName,
            StringAttributes =
            {
                ["prefix"] = Prefix,
                ["URI"] = NamespaceUri
            }
        };

        return ast;
    }

    public override string ToString()
    {
        return $"Q{{{NamespaceUri ?? ""}}}{(Prefix == "" ? "" : Prefix + ":") + LocalName}";
    }
}