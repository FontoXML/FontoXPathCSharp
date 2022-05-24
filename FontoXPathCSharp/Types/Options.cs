using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.NodesFactory;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc = System.Func<LexicalQualifiedName, int, ResolvedQualifiedName>;
using LoggingFunc = System.Action<string>;

public class LexicalQualifiedName
{
    public LexicalQualifiedName(string localName, string prefix)
    {
        LocalName = localName;
        Prefix = prefix;
    }

    public string LocalName { get; }

    public string Prefix { get; }
}

public class ResolvedQualifiedName
{
    public ResolvedQualifiedName(string localName, string namespaceUri)
    {
        LocalName = localName;
        NamespaceUri = namespaceUri;
    }

    public string LocalName { get; }

    public string NamespaceUri { get; }
}


public class Language
{
    public enum LanguageID
    {
        XPATH_3_1_LANGUAGE,
        XQUERY_3_1_LANGUAGE,
        XQUERY_UPDATE_3_1_LANGUAGE
    }

    private string GetLanguageName(LanguageID lang)
    {
        switch (lang)
        {
            case LanguageID.XPATH_3_1_LANGUAGE:
                return "XPath3.1";
            case LanguageID.XQUERY_3_1_LANGUAGE:
                return "XQuery3.1";
            case LanguageID.XQUERY_UPDATE_3_1_LANGUAGE:
                return "XQueryUpdate3.1";
            default:
                throw new Exception("Unreachable");
        }
    }
}

public class Options
{
    public Options(
        bool debug = false,
        bool disableCache = false,
        string defaultFunctionNamespaceUri = null,
        object? currentContext = null,
        IDocumentWriter? documentWriter = null,
        Language.LanguageID? languageId = null,
        Dictionary<string, string>? moduleImports = null,
        NamespaceResolverFunc? namespaceResolver = null,
        FunctionNameResolverFunc? functionNameResolver = null
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

    public Language.LanguageID? LanguageId { get; set; }

    public Dictionary<string, string>? ModuleImports { get; set; }

    public string? DefaultFunctionNamespaceUri { get; set; }

    public NamespaceResolverFunc? NamespaceResolver { get; set; }
    public FunctionNameResolverFunc? FunctionNameResolver { get; set; }

    public INodesFactory? NodesFactory { get; set; } = null;

    public LoggingFunc? Logger { get; set; } = null;
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