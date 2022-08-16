using System;
using System.Collections.Generic;
using System.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;

namespace XPathTest.Qt3Tests;

public record Qt3TestEnvironment<TNode>
{
    public Qt3TestEnvironment(
        string? baseUrl,
        TNode environmentNode,
        IDomFacade<TNode> domFacade,
        NodeUtils<TNode> nodeUtils,
        Options<TNode> options)
    {
        var fileName = Evaluate.EvaluateXPathToString(
            "source[@role=\".\"]/@file",
            environmentNode,
            domFacade,
            options);

        var variables = Evaluate.EvaluateXPathToNodes(
                "source[@role!=\".\"]",
                environmentNode,
                domFacade,
                options)
            .Select(variable => new KeyValuePair<string, object>(
                Evaluate.EvaluateXPathToString(
                    "@role",
                    variable,
                    domFacade,
                    options)?[1..] ?? string.Empty,
                TestingUtils.LoadFileToString(
                    (baseUrl != null ? baseUrl + "/" : "") + Evaluate.EvaluateXPathToString(
                        "@file",
                        variable,
                        domFacade,
                        options)
                ) ?? string.Empty))
            .DistinctBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);
        var contextNode = !string.IsNullOrEmpty(fileName) ? nodeUtils.LoadFileToXmlNode(fileName) : default;

        // TODO: ehh... no idea what is going on with that nested EvaluateXPath that's in the original.
        // Evaluate.EvaluateXPathToNodes("param", environmentNode).ToList().ForEach(paramNode => {
        //     variables[Evaluate.EvaluateXPathToString("@name", paramNode)] = Evaluate.EvaluateXPath<>()evaluateXPath(
        //         Evaluate.EvaluateXPathToString("@select", paramNode)
        //     );
        //     // tslint:disable-next-line: no-console
        //     console.log(variables);
        // });

        // TODO: Integrate namespace resolver here.

        ContextNode = contextNode;
        NamespaceResolver = null;
        Variables = variables;
    }

    public Qt3TestEnvironment(
        TNode? contextNode,
        Func<string, string>? namespaceResolver,
        Dictionary<string, object>? variables)
    {
        ContextNode = contextNode;
        NamespaceResolver = namespaceResolver;
        Variables = variables;
    }

    public TNode? ContextNode { get; init; }
    public Func<string, string>? NamespaceResolver { get; init; }
    public Dictionary<string, object>? Variables { get; init; }

    public void Deconstruct(
        out TNode? contextNode,
        out Func<string, string>? resolver,
        out Dictionary<string, object>? variables)
    {
        contextNode = ContextNode;
        resolver = NamespaceResolver;
        variables = Variables;
    }
}