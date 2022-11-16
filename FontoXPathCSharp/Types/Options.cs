using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.NodesFactory;
using LoggingFunc = System.Action<string>;

namespace FontoXPathCSharp.Types;

public record LexicalQualifiedName(string LocalName, string? Prefix);

public record ResolvedQualifiedName(string LocalName, string NamespaceUri);

public static class Language
{
    public enum LanguageId
    {
        Xpath31Language,
        Xquery31Language,
        XqueryUpdate31Language
    }

    public static LanguageId StringToLanguageId(string langName)
    {
        return langName switch
        {
            "XPath3.1" => LanguageId.Xpath31Language,
            "XQuery3.1" => LanguageId.Xquery31Language,
            "XQueryUpdate3.1" => LanguageId.XqueryUpdate31Language,
            _ => throw new Exception($"Invalid language ID '{langName}'")
        };
    }

    public static string GetLanguageName(LanguageId lang)
    {
        return lang switch
        {
            LanguageId.Xpath31Language => "XPath3.1",
            LanguageId.Xquery31Language => "XQuery3.1",
            LanguageId.XqueryUpdate31Language => "XQueryUpdate3.1",
            _ => throw new Exception("Unreachable")
        };
    }
}

public delegate string XmlSerializerFunc<TNode>(TNode root);

public class Options<TNode> where TNode : notnull
{
    public Options(
        NamespaceResolver namespaceResolver,
        bool debug = false,
        bool disableCache = false,
        string? defaultFunctionNamespaceUri = null,
        object? currentContext = null,
        IDocumentWriter<TNode>? documentWriter = null,
        Language.LanguageId? languageId = null,
        Dictionary<string, string>? moduleImports = null,
        FunctionNameResolver? functionNameResolver = null,
        XmlSerializerFunc<TNode>? xmlSerializer = null
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
        XmlSerializer = xmlSerializer;
    }

    public object? CurrentContext { get; set; }

    public bool Debug { get; set; }

    public bool DisableCache { get; set; }

    public IDocumentWriter<TNode>? DocumentWriter { get; set; }

    public Language.LanguageId? LanguageId { get; set; }

    public Dictionary<string, string>? ModuleImports { get; set; }

    public string? DefaultFunctionNamespaceUri { get; set; }

    public NamespaceResolver NamespaceResolver { get; set; }

    public FunctionNameResolver? FunctionNameResolver { get; set; }

    public INodesFactory<TNode>? NodesFactory { get; set; }

    public LoggingFunc? Logger { get; set; }

    public XmlSerializerFunc<TNode>? XmlSerializer { get; }
}