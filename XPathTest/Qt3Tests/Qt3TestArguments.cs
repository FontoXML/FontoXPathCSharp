using System.Collections.Generic;
using System.IO;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using XPathTest.Caches;

namespace XPathTest.Qt3Tests;

public class Qt3TestArguments<TNode> where TNode : notnull
{
    public Qt3TestArguments(
        string baseUrl,
        TNode? contextNode,
        IDomFacade<TNode> domFacade,
        string testQuery,
        Language.LanguageId language,
        NamespaceResolver namespaceResolver,
        Dictionary<string, object>? variablesInScope)
    {
        BaseUrl = baseUrl;
        ContextNode = contextNode;
        DomFacade = domFacade;
        TestQuery = testQuery;
        Language = language;
        NamespaceResolver = namespaceResolver;
        VariablesInScope = variablesInScope;
    }

    public Qt3TestArguments(
        string testSetFileName,
        TNode testCase,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        INodeUtils<TNode> nodeUtils)
    {
        var baseUrl = testSetFileName[..testSetFileName.LastIndexOf('/')];
        var filename = Evaluate.EvaluateXPathToString("test/@file", testCase, domFacade, options);
        var filepath = $"{baseUrl}/{filename}";

        string? testQuery;
        if (Evaluate.EvaluateXPathToBoolean("./test/@file", testCase, domFacade, options))
        {
            if (TestFileSystem.FileExists(filepath))
                testQuery = TestingUtils.LoadQt3TestFileToString(Evaluate.EvaluateXPathToString(
                        $"{baseUrl} || '/' || test/@file",
                        testCase,
                        domFacade,
                        options) ?? ""
                );
            else
                throw new FileNotFoundException($"Could not load file {filepath}");
        }
        else
        {
            testQuery = Evaluate.EvaluateXPathToString("./test", testCase, domFacade, options)!;
        }

        var language = FontoXPathCSharp.Types.Language.LanguageId.Xpath31Language;

        //TODO: Uncomment when below query is supported.
        // var languageString = Evaluate.EvaluateXPathToString(
        //     "if (((dependency)[@type = 'spec']/@value)!tokenize(.) = ('XQ10+', 'XQ30+', 'XQ31+', 'XQ31')) " +
        //     "then 'XQuery3.1' else if (((dependency)[@type = 'spec']/@value)!tokenize(.) = ('XP20', 'XP20+', 'XP30', 'XP30+')) " +
        //     "then 'XPath3.1' else if (((../dependency)[@type = 'spec']/@value)!tokenize(.) = ('XQ10+', 'XQ30+', 'XQ31+', 'XQ31')) " +
        //     "then 'XQuery3.1' else 'XPath3.1'",
        //     testCase,
        //     domFacade,
        //     options
        // );
        // var language = FontoXPathCSharp.Types.Language.StringToLanguageId(languageString);


        //TODO: Retrieve namespaces from the test case.
        var namespaces = new Dictionary<string, string>();

        var localNamespaceResolver = namespaces.Count != 0
            ? new NamespaceResolver(prefix => namespaces[prefix!])
            : null;

        var environmentNode = Evaluate.EvaluateXPathToFirstNode(
            "let $ref := ./environment/@ref return if ($ref) then /test-set/environment[@name = $ref] else ./environment",
            testCase,
            domFacade,
            options);

        var env = environmentNode != null
            ? new Qt3TestEnvironment<TNode>(baseUrl, environmentNode, domFacade, nodeUtils, options)
            : EnvironmentsByNameCache<TNode>.Instance.GetResource(
                Evaluate.EvaluateXPathToString("./environment/@ref", testCase, domFacade, options) ?? "empty")!;

        // if(environmentNode == null && (environmentRef ?? "empty") == "empty") Console.WriteLine("Environment ContextNode: " + env.ContextNode);

        var resolver = env.NamespaceResolver;

        NamespaceResolver namespaceResolver = localNamespaceResolver != null
            ? prefix => localNamespaceResolver(prefix) ?? resolver(prefix!)
            : prefix => resolver(prefix!);

        BaseUrl = Path.GetDirectoryName(filepath) ?? string.Empty;
        ContextNode = env.ContextNode;
        DomFacade = domFacade;
        TestQuery = testQuery!;
        Language = language;
        NamespaceResolver = namespaceResolver;
        VariablesInScope = env.Variables;
    }

    public string BaseUrl { get; }
    public TNode? ContextNode { get; }
    public IDomFacade<TNode> DomFacade { get; }
    public string TestQuery { get; }
    public Language.LanguageId Language { get; }
    public NamespaceResolver NamespaceResolver { get; }
    public Dictionary<string, object>? VariablesInScope { get; }

    public void Deconstruct(
        out string baseUrl,
        out TNode? contextNode,
        out IDomFacade<TNode> domFacade,
        out string testQuery,
        out Language.LanguageId language,
        out NamespaceResolver? namespaceResolver,
        out Dictionary<string, object>? variablesInScope)
    {
        baseUrl = BaseUrl;
        contextNode = ContextNode;
        domFacade = DomFacade;
        testQuery = TestQuery;
        language = Language;
        namespaceResolver = NamespaceResolver;
        variablesInScope = VariablesInScope;
    }
}