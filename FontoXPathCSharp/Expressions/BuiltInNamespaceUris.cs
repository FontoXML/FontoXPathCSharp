namespace FontoXPathCSharp.Expressions;

public enum BuiltInNamespaceUris
{
    XmlnsNamespaceUri,
    XmlNamespaceUri,
    XmlSchemaNamespaceUri,
    ArrayNamespaceUri,
    FunctionsNamespaceUri,
    LocalNamespaceUri,
    MapNamespaceUri,
    MathNamespaceUri,
    FontoxpathNamespaceUri,
    XqueryxUpdatingNamespaceUri,
    XqueryxNamespaceUri,
}

public static class StaticallyKnownNamespacesExtensions
{
    public static string GetUri(this BuiltInNamespaceUris knownNamespace)
    {
        return knownNamespace switch
        {
            BuiltInNamespaceUris.XmlnsNamespaceUri => "http://www.w3.org/2000/xmlns/",
            BuiltInNamespaceUris.XmlNamespaceUri => "http://www.w3.org/XML/1998/namespace",
            BuiltInNamespaceUris.XmlSchemaNamespaceUri => "http://www.w3.org/2001/XMLSchema",
            BuiltInNamespaceUris.ArrayNamespaceUri => "http://www.w3.org/2005/xpath-functions/array",
            BuiltInNamespaceUris.FunctionsNamespaceUri => "http://www.w3.org/2005/xpath-functions",
            BuiltInNamespaceUris.LocalNamespaceUri => "http://www.w3.org/2005/xquery-local-functions",
            BuiltInNamespaceUris.MapNamespaceUri => "http://www.w3.org/2005/xpath-functions/map",
            BuiltInNamespaceUris.MathNamespaceUri => "http://www.w3.org/2005/xpath-functions/math",
            BuiltInNamespaceUris.FontoxpathNamespaceUri => "http://fontoxml.com/fontoxpath",
            BuiltInNamespaceUris.XqueryxUpdatingNamespaceUri => "http://www.w3.org/2007/xquery-update-10",
            BuiltInNamespaceUris.XqueryxNamespaceUri => "http://www.w3.org/2005/XQueryX",
            _ => throw new ArgumentOutOfRangeException(nameof(knownNamespace), knownNamespace, null)
        };
    }

    private static Dictionary<string, string> _staticallyKnownNamespaceByPrefix = new()
    {
        { "xml", BuiltInNamespaceUris.XmlNamespaceUri.GetUri() },
        { "xs", BuiltInNamespaceUris.XmlSchemaNamespaceUri.GetUri() },
        { "fn", BuiltInNamespaceUris.FunctionsNamespaceUri.GetUri() },
        { "map", BuiltInNamespaceUris.MapNamespaceUri.GetUri() },
        { "array", BuiltInNamespaceUris.ArrayNamespaceUri.GetUri() },
        { "math", BuiltInNamespaceUris.MathNamespaceUri.GetUri() },
        { "fontoxpath", BuiltInNamespaceUris.FontoxpathNamespaceUri.GetUri() },
        { "local", BuiltInNamespaceUris.LocalNamespaceUri.GetUri() },
    };

    public static string? GetStaticallyKnownNamespaceByPrefix(string prefix)
    {
        return !_staticallyKnownNamespaceByPrefix.ContainsKey(prefix)
            ? null
            : _staticallyKnownNamespaceByPrefix[prefix];
    }

    public static void RegisterStaticallyKnownNamespace(string prefix, string namespaceUri)
    {
        if (_staticallyKnownNamespaceByPrefix.ContainsKey(prefix))
            throw new Exception("Prefix already registered: Do not register the same prefix twice.");
        _staticallyKnownNamespaceByPrefix.Add(prefix, namespaceUri);
    }
}