using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.NodesFactory;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc =
    System.Func<FontoXPathCSharp.Types.LexicalQualifiedName, int, FontoXPathCSharp.Types.ResolvedQualifiedName>;
using LoggingFunc = System.Action<string>;

namespace FontoXPathCSharp.Types;

public record LexicalQualifiedName(string LocalName, string? Prefix);

public record ResolvedQualifiedName(string LocalName, string NamespaceUri);

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

    public Language.LanguageId? LanguageId { get; set; }

    public Dictionary<string, string>? ModuleImports { get; set; }

    public string? DefaultFunctionNamespaceUri { get; set; }

    public NamespaceResolverFunc? NamespaceResolver { get; set; }
    public FunctionNameResolverFunc? FunctionNameResolver { get; set; }

    public INodesFactory? NodesFactory { get; set; } = null;

    public LoggingFunc? Logger { get; set; } = null;
}

public record CompilationOptions(bool AllowUpdating, bool AllowXQuery, bool Debug, bool DisableCache);