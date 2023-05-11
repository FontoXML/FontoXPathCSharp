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
            "source[@role='.']/@file",
            environmentNode,
            domFacade,
            options);
        
        var variables = Evaluate.EvaluateXPathToNodes(
                "source[@role!='.']",
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

                var testFile = nodeUtils.StringToXmlDocument(TestingUtils.LoadQt3TestFileToString(
                    baseUrl != null ? Path.Combine(baseUrl, filePath) : filePath
                ) ?? "");
                return new KeyValuePair<string, object>(
                    Evaluate.EvaluateXPathToString(
                        "@role",
                        variable,
                        domFacade,
                        options)[1..] ?? string.Empty,
                     testFile);
            })
            .DistinctBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);

        
        var contextNode = !string.IsNullOrEmpty(fileName)
            ? nodeUtils.LoadFileToXmlNode(baseUrl != null ? Path.Combine(baseUrl, fileName) : fileName)
            : nodeUtils.CreateDocument();

        // Evaluate.EvaluateXPathToNodes("param", environmentNode, domFacade, options).ToList().ForEach(paramNode =>
        // {
        //     var name = Evaluate.EvaluateXPathToString("@name", paramNode, domFacade, options);
        //     if (name != null)
        //     {
        //         variables[name] = Evaluate.EvaluateXPathToString(
        //             Evaluate.EvaluateXPathToAny("@select", paramNode, domFacade, options)!, paramNode, domFacade, options
        //         )!;
        //         // tslint:disable-next-line: no-console
        //         Console.WriteLine(variables);
        //     }
        // });

        var namespacesEntries = Evaluate.EvaluateXPathToNodes("./namespace", environmentNode, domFacade, options);
        var namespaces = new Dictionary<string, string>();
        foreach (var ns in namespacesEntries)
        {
            var prefix = Evaluate.EvaluateXPathToString("@prefix", ns, domFacade, options);
            var uri = Evaluate.EvaluateXPathToString("@uri", ns, domFacade, options);
            namespaces.Add(prefix, uri);
        }

        ContextNode = contextNode;
        NamespaceResolver = prefix => namespaces.ContainsKey(prefix) ? namespaces[prefix] : null;
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