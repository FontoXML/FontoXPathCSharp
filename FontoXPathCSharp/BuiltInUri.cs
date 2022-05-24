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
    private static readonly Dictionary<BuiltInUri, string> builtInNamespaceUris = new()
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

    private static readonly Dictionary<string, string?> prefixUriLookup = new()
    {
        ["xml"] = GetBuiltinNamespaceURI(BuiltInUri.XML_NAMESPACE_URI),
        ["xs"] = GetBuiltinNamespaceURI(BuiltInUri.XMLSCHEMA_NAMESPACE_URI),
        ["fn"] = GetBuiltinNamespaceURI(BuiltInUri.FUNCTIONS_NAMESPACE_URI),
        ["map"] = GetBuiltinNamespaceURI(BuiltInUri.MAP_NAMESPACE_URI),
        ["array"] = GetBuiltinNamespaceURI(BuiltInUri.ARRAY_NAMESPACE_URI),
        ["math"] = GetBuiltinNamespaceURI(BuiltInUri.MATH_NAMESPACE_URI),
        ["fontoxpath"] = GetBuiltinNamespaceURI(BuiltInUri.FONTOXPATH_NAMESPACE_URI),
        ["local"] = GetBuiltinNamespaceURI(BuiltInUri.LOCAL_NAMESPACE_URI)
    };

    public static string GetBuiltinNamespaceURI(this BuiltInUri builtInUri)
    {
        if (builtInNamespaceUris.ContainsKey(builtInUri)) return builtInNamespaceUris[builtInUri];
        throw new Exception("Built in URI not known: " + builtInUri);
    }

    public static string? GetStaticallyKnownNamespaceByPrefix(string prefix)
    {
        if (prefixUriLookup.ContainsKey(prefix)) return prefixUriLookup[prefix];
        return null;
    }

    public static void RegisterStaticallyKnownNamespace(string prefix, string? namespaceURI)
    {
        if (prefixUriLookup.ContainsKey(prefix))
            throw new Exception("Prefix already registered: Do not register the same prefix twice.");
        prefixUriLookup[prefix] = namespaceURI;
    }
}