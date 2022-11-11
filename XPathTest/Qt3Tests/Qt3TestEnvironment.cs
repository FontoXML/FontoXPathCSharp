using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;

namespace XPathTest.Qt3Tests;

public record Qt3TestEnvironment<TNode> where TNode : notnull
{
    public Qt3TestEnvironment(
        string? baseUrl,
        TNode environmentNode,
        IDomFacade<TNode> domFacade,
        INodeUtils<TNode> nodeUtils,
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
            .Select(variable =>
            {
                var filePath = Evaluate.EvaluateXPathToString(
                    "@file",
                    variable,
                    domFacade,
                    options)!;
                return new KeyValuePair<string, object>(
                    Evaluate.EvaluateXPathToString(
                        "@role",
                        variable,
                        domFacade,
                        options)?[1..] ?? string.Empty,
                    TestingUtils.LoadQt3TestFileToString(
                        baseUrl != null ? Path.Combine(baseUrl, filePath) : filePath
                    ) ?? string.Empty);
            })
            .DistinctBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);

        // Console.WriteLine($"FILE NAME:{fileName} BASE URL {baseUrl}");
        var contextNode = !string.IsNullOrEmpty(fileName)
            ? nodeUtils.LoadFileToXmlNode(baseUrl != null ? Path.Combine(baseUrl, fileName) : fileName)
            : default;

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
        NamespaceResolver = _ => null;
        Variables = variables;
    }

    public Qt3TestEnvironment(
        TNode? contextNode,
        Func<string, string?> namespaceResolver,
        Dictionary<string, object>? variables)
    {
        ContextNode = contextNode;
        NamespaceResolver = namespaceResolver;
        Variables = variables;
    }

    public TNode? ContextNode { get; }
    public Func<string, string?> NamespaceResolver { get; }
    public Dictionary<string, object>? Variables { get; }

    public void Deconstruct(
        out TNode? contextNode,
        out Func<string, string?> namespaceResolver,
        out Dictionary<string, object>? variables)
    {
        contextNode = ContextNode;
        namespaceResolver = NamespaceResolver;
        variables = Variables;
    }
}