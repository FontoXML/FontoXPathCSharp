namespace FontoXPathCSharp;

public enum BuiltInUri
{
    XmlnsNamespaceUri,
    XmlNamespaceUri,
    XmlschemaNamespaceUri,
    ArrayNamespaceUri,
    FunctionsNamespaceUri,
    LocalNamespaceUri,
    MapNamespaceUri,
    MathNamespaceUri,
    FontoxpathNamespaceUri,
    XqueryxUpdatingNamespaceUri,
    XqueryxNamespaceUri
}

public static class StaticallyKnownNamespaceUtils
{
    private static readonly Dictionary<BuiltInUri, string> BuiltInNamespaceUris = new()
    {
        [BuiltInUri.XmlnsNamespaceUri] = "http://www.w3.org/2000/xmlns/",
        [BuiltInUri.XmlNamespaceUri] = "http://www.w3.org/XML/1998/namespace",
        [BuiltInUri.XmlschemaNamespaceUri] = "http://www.w3.org/2001/XMLSchema",
        [BuiltInUri.ArrayNamespaceUri] = "http://www.w3.org/2005/xpath-functions/array",
        [BuiltInUri.FunctionsNamespaceUri] = "http://www.w3.org/2005/xpath-functions",
        [BuiltInUri.LocalNamespaceUri] = "http://www.w3.org/2005/xquery-local-functions",
        [BuiltInUri.MapNamespaceUri] = "http://www.w3.org/2005/xpath-functions/map",
        [BuiltInUri.MathNamespaceUri] = "http://www.w3.org/2005/xpath-functions/math",
        [BuiltInUri.FontoxpathNamespaceUri] = "http://fontoxml.com/fontoxpath",
        [BuiltInUri.XqueryxUpdatingNamespaceUri] = "http://www.w3.org/2007/xquery-update-10",
        [BuiltInUri.XqueryxNamespaceUri] = "http://www.w3.org/2005/XQueryX"
    };

    private static readonly Dictionary<string, string?> PrefixUriLookup = new()
    {
        ["xml"] = GetBuiltinNamespaceUri(BuiltInUri.XmlNamespaceUri),
        ["xs"] = GetBuiltinNamespaceUri(BuiltInUri.XmlschemaNamespaceUri),
        ["fn"] = GetBuiltinNamespaceUri(BuiltInUri.FunctionsNamespaceUri),
        ["map"] = GetBuiltinNamespaceUri(BuiltInUri.MapNamespaceUri),
        ["array"] = GetBuiltinNamespaceUri(BuiltInUri.ArrayNamespaceUri),
        ["math"] = GetBuiltinNamespaceUri(BuiltInUri.MathNamespaceUri),
        ["fontoxpath"] = GetBuiltinNamespaceUri(BuiltInUri.FontoxpathNamespaceUri),
        ["local"] = GetBuiltinNamespaceUri(BuiltInUri.LocalNamespaceUri)
    };

    public static string GetBuiltinNamespaceUri(this BuiltInUri builtInUri)
    {
        if (BuiltInNamespaceUris.ContainsKey(builtInUri)) return BuiltInNamespaceUris[builtInUri];
        throw new Exception("Built in URI not known: " + builtInUri);
    }

    public static string? GetStaticallyKnownNamespaceByPrefix(string? prefix)
    {
        return prefix != null && PrefixUriLookup.ContainsKey(prefix) ? PrefixUriLookup[prefix] : null;
    }

    public static void RegisterStaticallyKnownNamespace(string prefix, string? namespaceUri)
    {
        if (PrefixUriLookup.ContainsKey(prefix))
            throw new Exception("Prefix already registered: Do not register the same prefix twice.");
        PrefixUriLookup[prefix] = namespaceUri;
    }
}