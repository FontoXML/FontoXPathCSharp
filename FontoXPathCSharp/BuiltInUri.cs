namespace FontoXPathCSharp;

public enum BuiltInUri
{
    XMLNS_NAMESPACE_URI,
    XML_NAMESPACE_URI,
    XMLSCHEMA_NAMESPACE_URI,
    ARRAY_NAMESPACE_URI,
    FUNCTIONS_NAMESPACE_URI,
    LOCAL_NAMESPACE_URI,
    MAP_NAMESPACE_URI,
    MATH_NAMESPACE_URI,
    FONTOXPATH_NAMESPACE_URI,
    XQUERYX_UPDATING_NAMESPACE_URI,
    XQUERYX_NAMESPACE_URI
}

internal static class StaticallyKnownNamespaceUtils
{
    private static readonly Dictionary<BuiltInUri, string> BuiltInNamespaceUris = new()
    {
        [BuiltInUri.XMLNS_NAMESPACE_URI] = "http://www.w3.org/2000/xmlns/",
        [BuiltInUri.XML_NAMESPACE_URI] = "http://www.w3.org/XML/1998/namespace",
        [BuiltInUri.XMLSCHEMA_NAMESPACE_URI] = "http://www.w3.org/2001/XMLSchema",
        [BuiltInUri.ARRAY_NAMESPACE_URI] = "http://www.w3.org/2005/xpath-functions/array",
        [BuiltInUri.FUNCTIONS_NAMESPACE_URI] = "http://www.w3.org/2005/xpath-functions",
        [BuiltInUri.LOCAL_NAMESPACE_URI] = "http://www.w3.org/2005/xquery-local-functions",
        [BuiltInUri.MAP_NAMESPACE_URI] = "http://www.w3.org/2005/xpath-functions/map",
        [BuiltInUri.MATH_NAMESPACE_URI] = "http://www.w3.org/2005/xpath-functions/math",
        [BuiltInUri.FONTOXPATH_NAMESPACE_URI] = "http://fontoxml.com/fontoxpath",
        [BuiltInUri.XQUERYX_UPDATING_NAMESPACE_URI] = "http://www.w3.org/2007/xquery-update-10",
        [BuiltInUri.XQUERYX_NAMESPACE_URI] = "http://www.w3.org/2005/XQueryX"
    };

    private static readonly Dictionary<string, string?> PrefixUriLookup = new()
    {
        ["xml"] = GetBuiltinNamespaceUri(BuiltInUri.XML_NAMESPACE_URI),
        ["xs"] = GetBuiltinNamespaceUri(BuiltInUri.XMLSCHEMA_NAMESPACE_URI),
        ["fn"] = GetBuiltinNamespaceUri(BuiltInUri.FUNCTIONS_NAMESPACE_URI),
        ["map"] = GetBuiltinNamespaceUri(BuiltInUri.MAP_NAMESPACE_URI),
        ["array"] = GetBuiltinNamespaceUri(BuiltInUri.ARRAY_NAMESPACE_URI),
        ["math"] = GetBuiltinNamespaceUri(BuiltInUri.MATH_NAMESPACE_URI),
        ["fontoxpath"] = GetBuiltinNamespaceUri(BuiltInUri.FONTOXPATH_NAMESPACE_URI),
        ["local"] = GetBuiltinNamespaceUri(BuiltInUri.LOCAL_NAMESPACE_URI)
    };

    public static string GetBuiltinNamespaceUri(this BuiltInUri builtInUri)
    {
        if (BuiltInNamespaceUris.ContainsKey(builtInUri)) return BuiltInNamespaceUris[builtInUri];
        throw new Exception("Built in URI not known: " + builtInUri);
    }

    public static string? GetStaticallyKnownNamespaceByPrefix(string prefix)
    {
        return PrefixUriLookup.ContainsKey(prefix) ? PrefixUriLookup[prefix] : null;
    }

    public static void RegisterStaticallyKnownNamespace(string prefix, string? namespaceUri)
    {
        if (PrefixUriLookup.ContainsKey(prefix))
            throw new Exception("Prefix already registered: Do not register the same prefix twice.");
        PrefixUriLookup[prefix] = namespaceUri;
    }
}