using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;

namespace XPathTest.Qt3Tests;

public class Qt3TestArguments<TNode>
{
    public Qt3TestArguments(
        string BaseUrl,
        TNode? ContextNode,
        IDomFacade<TNode> DomFacade,
        string TestQuery,
        Language.LanguageId Language,
        Func<string, string?>? NamespaceResolver,
        Dictionary<string, object>? VariablesInScope)
    {
        this.BaseUrl = BaseUrl;
        this.ContextNode = ContextNode;
        this.DomFacade = DomFacade;
        this.TestQuery = TestQuery;
        this.Language = Language;
        this.NamespaceResolver = NamespaceResolver;
        this.VariablesInScope = VariablesInScope;
    }

    public Qt3TestArguments(
        string testSetFileName,
        TNode testCase,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        NodeUtils<TNode> nodeUtils)
    {
        var baseUrl = testSetFileName[..testSetFileName.LastIndexOf('/')];

        string testQuery;
        var filename = Evaluate.EvaluateXPathToString("test/@file", testCase, domFacade, options);
        var filepath = $"{baseUrl}/{filename}";

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
            ? new Func<string, string?>(prefix => namespaces[prefix])
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
                ? new Qt3TestEnvironment<TNode>(baseUrl, environmentNode, domFacade, nodeUtils, options)
                : new Qt3TestEnvironment<TNode>(nodeUtils.CreateDocument(), s => null,
                    new Dictionary<string, object>());

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
        TestQuery = testQuery;
        Language = language;
        NamespaceResolver = namespaceResolver;
        VariablesInScope = variablesInScope;
    }

    public string BaseUrl { get; init; }
    public TNode? ContextNode { get; init; }
    public IDomFacade<TNode> DomFacade { get; init; }
    public string TestQuery { get; init; }
    public Language.LanguageId Language { get; init; }
    public Func<string, string?>? NamespaceResolver { get; init; }
    public Dictionary<string, object>? VariablesInScope { get; init; }

    public void Deconstruct(
        out string BaseUrl,
        out TNode? ContextNode,
        out IDomFacade<TNode> DomFacade,
        out string TestQuery,
        out Language.LanguageId Language,
        out Func<string, string?>? NamespaceResolver,
        out Dictionary<string, object>? VariablesInScope)
    {
        BaseUrl = this.BaseUrl;
        ContextNode = this.ContextNode;
        DomFacade = this.DomFacade;
        TestQuery = this.TestQuery;
        Language = this.Language;
        NamespaceResolver = this.NamespaceResolver;
        VariablesInScope = this.VariablesInScope;
    }
}