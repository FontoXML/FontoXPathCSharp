using System;
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
                testQuery = TestingUtils.LoadQt3TestFileToString(filepath);
            else
                throw new FileNotFoundException($"Could not load file {filepath}");
        }
        else
        {
            testQuery = Evaluate.EvaluateXPathToString("./test", testCase, domFacade, options)!;
        }
        
        //TODO: Retrieve the language from the test case.
        var language = FontoXPathCSharp.Types.Language.LanguageId.Xpath31Language;

        //TODO: Retrieve namespaces from the test case.
        var namespaces = new Dictionary<string, string>();

        var localNamespaceResolver = namespaces.Count != 0
            ? new NamespaceResolver(prefix => namespaces[prefix!])
            : null;
        
        // var environmentRef = Evaluate.EvaluateXPathToString("./environment/@ref", testCase, domFacade, options);
        // var environmentNode = environmentRef != null 
        //     ? Evaluate.EvaluateXPathToFirstNode($"/test-set/environment[@name = ${environmentRef}]", testCase, domFacade, options)
        //     : Evaluate.EvaluateXPathToFirstNode("./environment", testCase, domFacade, options);

        var environmentNode = Evaluate.EvaluateXPathToFirstNode(
            "let $ref := ./environment/@ref return if ($ref) then /test-set/environment[@name = $ref] else ./environment",
            testCase,
            domFacade,
            options);
        
        Console.WriteLine($"Environment Node is{(environmentNode != null ? " not " : " ")}null, Result: {(environmentNode != null ? domFacade.GetData(environmentNode!) : "")}");

        var env = environmentNode != null
            ? new Qt3TestEnvironment<TNode>(
                baseUrl,
                environmentNode,
                domFacade,
                nodeUtils,
                options
            )
            : EnvironmentsByNameCache<TNode>.Instance.GetResource(
                Evaluate.EvaluateXPathToString("./environment/@ref", testCase, domFacade, options) ?? "empty"
            )!;

        var contextNode = env.ContextNode;
        var resolver = env.NamespaceResolver;

        NamespaceResolver namespaceResolver = localNamespaceResolver != null
            ? prefix => localNamespaceResolver(prefix) ?? resolver(prefix!)
            : prefix => resolver(prefix!);

        BaseUrl = Path.GetDirectoryName(filepath) ?? string.Empty;
        ContextNode = contextNode;
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