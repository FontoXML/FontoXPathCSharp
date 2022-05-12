using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.NodesFactory;

namespace FontoXPathCSharp.Types;

public class LexicalQualifiedName
{
    public LexicalQualifiedName(string localName, string? prefix)
    {
        LocalName = localName;
        Prefix = prefix;
    }

    public string LocalName { get; }

    public string? Prefix { get; }
}

public class ResolvedQualifiedName
{
    public ResolvedQualifiedName(string localName, string namespaceUri)
    {
        LocalName = localName;
        NamespaceUri = namespaceUri;
    }

    public string LocalName { get; }

    public string? NamespaceUri { get; }
}


public static class Language
{
    public enum LanguageId
    {
        XPATH_3_1_LANGUAGE,
        XQUERY_3_1_LANGUAGE,
        XQUERY_UPDATE_3_1_LANGUAGE
    }

    private static string GetLanguageName(LanguageId lang)
    {
        return lang switch
        {
            LanguageId.XPATH_3_1_LANGUAGE => "XPath3.1",
            LanguageId.XQUERY_3_1_LANGUAGE => "XQuery3.1",
            LanguageId.XQUERY_UPDATE_3_1_LANGUAGE => "XQueryUpdate3.1",
            _ => throw new Exception("Unreachable")
        };
    }
}

public class Options
{
    public Options(
        bool debug = false,
        bool disableCache = false,
        string? defaultFunctionNamespaceUri = null,
        object? currentContext = null,
        IDocumentWriter? documentWriter = null,
        Language.LanguageId? languageId = null,
        Dictionary<string, string>? moduleImports = null,
        Func<string, string?>? namespaceResolver = null,
        Func<LexicalQualifiedName, int, ResolvedQualifiedName>? functionNameResolver = null
    )
    {
        CurrentContext = currentContext;
        Debug = debug;
        DefaultFunctionNamespaceUri = defaultFunctionNamespaceUri;
        DisableCache = disableCache;
        DocumentWriter = documentWriter;
        LanguageId = languageId;
        ModuleImports = moduleImports;
        NamespaceResolver = namespaceResolver;
        FunctionNameResolver = functionNameResolver;
    }

    public object? CurrentContext { get; set; }

    public bool Debug { get; set; }

    public bool DisableCache { get; set; }

    public IDocumentWriter? DocumentWriter { get; set; }

    public Language.LanguageId? LanguageId { get; set; }

    public Dictionary<string, string>? ModuleImports { get; set; }

    public string? DefaultFunctionNamespaceUri { get; set; }

    public Func<string, string?>? NamespaceResolver { get; set; }
    public Func<LexicalQualifiedName, int, ResolvedQualifiedName>? FunctionNameResolver { get; set; }

    public INodesFactory? NodesFactory { get; set; }

    public Action<string>? Logger { get; set; }
}

public class CompilationOptions
{
    public bool AllowUpdating;
    public bool AllowXQuery;
    public bool Debug;
    public bool DisableCache;

    public CompilationOptions(bool allowUpdating, bool allowXQuery, bool debug, bool disableCache)
    {
        AllowUpdating = allowUpdating;
        AllowXQuery = allowXQuery;
        Debug = debug;
        DisableCache = disableCache;
    }
}