using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using XPathTest.Caches;

namespace XPathTest;

public class TestingUtils
{
    /**
     * Takes in an container of entries and creates a list that contains the occurences per entry that is sorted descending by occurence.
     */
    public static List<KeyValuePair<T, int>> GetSortedValueOccurrences<T>(IEnumerable<T> list) where T : notnull
    {
        var occurenceList = list.Aggregate(
            new Dictionary<T, int>(),
            (acc, val) =>
            {
                if (!acc.ContainsKey(val)) acc[val] = 0;
                acc[val]++;
                return acc;
            }
        ).ToList();

        occurenceList.Sort((a, b) => a.Value.CompareTo(b.Value));
        occurenceList.Reverse();

        return occurenceList;
    }

    /**
     * Takes a set of key-value pairs, such as a dictionary, and writes it to disk in csv format,
     * delimited with commas for columns and newlines for rows.
     */
    public static void WriteKvpCollectionToDisk<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> dict,
        string fileName)
    {
        TestFileSystem.WriteFile(fileName, string.Join(
            Environment.NewLine,
            dict.Select(d => $"{d.Key},{d.Value}")
        ));
    }

    public static string PreprocessFilename(string filename)
    {
        while (filename.Contains(".."))
        {
            var parts = filename.Split('/');

            filename = string.Join('/', parts
                .Take(Array.IndexOf(parts, ".."))
                .Concat(parts.Skip(Array.IndexOf(parts, "..") + 1)));
        }

        return filename;
    }

    public static string? LoadFileToString(string filename)
    {
        return DocumentsByPathCache.Instance.GetResource($"qt3tests/{PreprocessFilename(filename)}");
    }

    public static TestArguments<TNode> GetArguments<TNode>(string testSetFileName, TNode testCase,
        IDomFacade<TNode> domFacade, Options<TNode> options, NodeUtils<TNode> nodeUtils)
    {
        var baseUrl = testSetFileName[..testSetFileName.LastIndexOf('/')];

        string testQuery;
        var filename = Evaluate.EvaluateXPathToString("test/@file", testCase, domFacade, options);
        var filepath = $"{baseUrl}/{filename}";

        if (Evaluate.EvaluateXPathToBoolean("./test/@file", testCase, domFacade, options))
        {
            if (TestFileSystem.FileExists(filepath))
                testQuery = LoadFileToString(filepath);
            else
                throw new FileNotFoundException($"Could not load file {filepath}");
        }
        else
        {
            testQuery = Evaluate.EvaluateXPathToString("./test", testCase, domFacade, options)!;
        }


        //TODO: Retrieve the language from the test case.
        var language = Language.LanguageId.XPATH_3_1_LANGUAGE;

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
                ? CreateEnvironment(baseUrl, environmentNode, domFacade, nodeUtils, options)
                : new Environment<TNode>(nodeUtils.CreateDocument(), s => null, new Dictionary<string, object>());

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

        return new TestArguments<TNode>(Path.GetDirectoryName(filepath), contextNode, domFacade, testQuery, language,
            namespaceResolver, variablesInScope!);
    }

    public static Environment<TNode> CreateEnvironment<TNode>(string? baseUrl, TNode environmentNode,
        IDomFacade<TNode> domFacade, NodeUtils<TNode> nodeUtils, Options<TNode> options)
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
                LoadFileToString(
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

        return new Environment<TNode>(contextNode, null, variables);
    }


    public static Dictionary<TKey, TValue> ToDictionarySafe<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>> enumerable) where TKey : notnull
    {
        return enumerable
            .DistinctBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public record Environment<TNode>(
        TNode? ContextNode,
        Func<string, string>? NamespaceResolver,
        Dictionary<string, object>? Variables
    );

    public record TestArguments<TNode>(
        string BaseUrl,
        TNode? ContextNode,
        IDomFacade<TNode> DomFacade,
        string TestQuery,
        Language.LanguageId Language,
        Func<string, string?>? NamespaceResolver,
        Dictionary<string, object>? VariablesInScope
    );
}