using System.Collections.Generic;
using System.IO;
using System.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;

namespace XPathTest.Qt3Tests;

public class Qt3TestArguments<TNode> where TNode : notnull
{
    public Qt3TestArguments(
        string baseUrl,
        TNode? contextNode,
        IDomFacade<TNode> domFacade,
        string testQuery,
        Language.LanguageId language,
        NamespaceResolver? namespaceResolver,
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
                testQuery = TestingUtils.LoadFileToString(filepath);
            else
                throw new FileNotFoundException($"Could not load file {filepath}");
        }
        else
        {
            testQuery = Evaluate.EvaluateXPathToString("./test", testCase, domFacade, options)!;
        }


        //TODO: Retrieve the language from the test case.
        var language = FontoXPathCSharp.Types.Language.LanguageId.XPATH_3_1_LANGUAGE;

        //TODO: Retrieve namespaces from the test case.
        var namespaces = new Dictionary<string, string>();

        var localNamespaceResolver = namespaces.Count != 0
            ? new NamespaceResolver(prefix => namespaces[prefix])
            : null;

        var namespaceResolver = localNamespaceResolver;

        var refString = Evaluate.EvaluateXPathToString(
            "./environment/@ref",
            testCase,
            domFacade,
            options
        );
        var environmentNodes =
            Evaluate.EvaluateXPathToNodes(
                "./environment",
                testCase,
                domFacade,
                options
            );
        var environmentNode = environmentNodes.Any()
            ? Evaluate.EvaluateXPathToFirstNode(
                $"/test-set/environment[@name = \"{refString}\"]",
                testCase,
                domFacade,
                options)
            : Evaluate.EvaluateXPathToFirstNode(
                "./environment",
                testCase,
                domFacade,
                options);


        var (contextNode, resolver, variablesInScope) =
            environmentNode != null
                ? new Qt3TestEnvironment<TNode>(
                    baseUrl,
                    environmentNode,
                    domFacade,
                    nodeUtils,
                    options
                )
                : new Qt3TestEnvironment<TNode>(
                    nodeUtils.CreateDocument(),
                    _ => null,
                    new Dictionary<string, object>()
                );

        // var (contextNode, resolver, variablesInScope) = (environmentNode != null
        //     ? CreateEnvironment(baseUrl, environmentNode, domFacade, nodeUtils, options)
        //     : EnvironmentsByNameCache<TNode>.Instance.GetResource(
        //         Evaluate.EvaluateXPathToString(
        //             "(./environment/@ref, \"empty\")[1]",
        //             testCase,
        //             domFacade,
        //             options) ?? string.Empty
        //     ))!;

        namespaceResolver = localNamespaceResolver != null
            ? prefix => localNamespaceResolver(prefix) ?? resolver?.Invoke(prefix)
            : prefix => resolver?.Invoke(prefix);

        BaseUrl = Path.GetDirectoryName(filepath) ?? string.Empty;
        ContextNode = contextNode;
        DomFacade = domFacade;
        TestQuery = testQuery!;
        Language = language;
        NamespaceResolver = namespaceResolver;
        VariablesInScope = variablesInScope;
    }

    public string BaseUrl { get; init; }
    public TNode? ContextNode { get; init; }
    public IDomFacade<TNode> DomFacade { get; init; }
    public string TestQuery { get; init; }
    public Language.LanguageId Language { get; init; }
    public NamespaceResolver? NamespaceResolver { get; init; }
    public Dictionary<string, object>? VariablesInScope { get; init; }

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