using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using XPathTest.Caches;

namespace XPathTest;

public static class Qt3TestUtils
{
    public static XmlNode StringToXmlNode(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        return doc;
    }
    
    private static string PreprocessFilename(string filename)
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

    public static XmlNode? LoadFileToXmlNode(string filename)
    {
        return XmlDocumentsByPathCache.Instance.GetResource(PreprocessFilename(filename));
    }

    public static string? LoadFileToString(string filename)
    {
        return DocumentsByPathCache.Instance.GetResource($"QT3TS/{PreprocessFilename(filename)}");
    }

    public static TestArguments GetArguments(string testSetFileName, XmlNode testCase)
    {
        // return new TestArguments("", testCase, "", Language.LanguageId.XPATH_3_1_LANGUAGE, null,
        //     new Dictionary<string, AbstractValue>());
        var baseUrl = testSetFileName.Substring(0, testSetFileName.LastIndexOf('/'));

        string testQuery;
        if (Evaluate.EvaluateXPathToBoolean("./test/@file", testCase, null, new Dictionary<string, AbstractValue>(),
                new Options()))
        {
            var filepath = $"{baseUrl}/{Evaluate.EvaluateXPathToString("test/@file", testCase)}";
            if (TestFileSystem.FileExists(filepath))
            {
                testQuery = LoadFileToString(filepath);
            }
            else
            {
                throw new FileNotFoundException($"Could not load file {filepath}");
            }
        }
        else
        {
            testQuery = Evaluate.EvaluateXPathToString("./test", testCase, null,
                new Dictionary<string, AbstractValue>(), new Options());
        }


        //TODO: Retrieve the language from the test case.
        var language = Language.LanguageId.XPATH_3_1_LANGUAGE;

        //TODO: Retrieve namespaces from the test case.
        var namespaces = new Dictionary<string, string>();

        var localNamespaceResolver = namespaces.Count != 0
            ? new Func<string, string?>(prefix => namespaces[prefix])
            : null;

        var namespaceResolver = localNamespaceResolver;

        var refString = Evaluate.EvaluateXPathToString("./environment/@ref", testCase);
        var environmentNodes = Evaluate.EvaluateXPathToNodes("./environment", testCase);
        var environmentNode = environmentNodes.Any()
            ? Evaluate.EvaluateXPathToFirstNode($"/test-set/environment[@name = \"{refString}\"]", testCase)
            : Evaluate.EvaluateXPathToFirstNode("./environment", testCase);

        var env = CreateEnvironment(baseUrl, environmentNode);

        // var env = environmentNode != null
        //     ? CreateEnvironment(baseUrl, environmentNode)
        //     : EnvironmentsByNameCache.Instance.GetResource(
        //         Evaluate.EvaluateXPathToString("(./environment/@ref, \"empty\")[1]", testCase));

        var contextNode = env.ContextNode;
        namespaceResolver = localNamespaceResolver != null
            ? prefix => localNamespaceResolver(prefix) ?? env.NamespaceResolver?.Invoke(prefix)
            : prefix => env.NamespaceResolver?.Invoke(prefix);

        var variablesInScope = env.Variables;

        return new TestArguments(baseUrl, contextNode, testQuery, language, namespaceResolver, variablesInScope!);
    }

    public static Environment CreateEnvironment(string? baseUrl, XmlNode environmentNode)
    {
        var fileName = Evaluate.EvaluateXPathToString("source[@role=\".\"]/@file", environmentNode);
        var variables = Evaluate.EvaluateXPathToNodes("source[@role!=\".\"]", environmentNode)
            .Select(variable => new KeyValuePair<string, AbstractValue?>(
                Evaluate.EvaluateXPathToString("@role", variable)[1..],
                new StringValue(LoadFileToString(
                    (baseUrl != null ? baseUrl + "/" : "") + Evaluate.EvaluateXPathToString("@file", variable)
                ) ?? string.Empty)))
            .DistinctBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);
        var contextNode = (fileName != null && fileName.Length > 0) ? LoadFileToXmlNode(fileName) : null;

        // TODO: ehh... no idea what is going on with that nested EvaluateXPath that's in the original.
        // Evaluate.EvaluateXPathToNodes("param", environmentNode).ToList().ForEach(paramNode => {
        //     variables[Evaluate.EvaluateXPathToString("@name", paramNode)] = Evaluate.EvaluateXPath<>()evaluateXPath(
        //         Evaluate.EvaluateXPathToString("@select", paramNode)
        //     );
        //     // tslint:disable-next-line: no-console
        //     console.log(variables);
        // });

        // TODO: Integrate namespace resolver here.

        return new Environment(contextNode, null, variables);
    }


    public static Dictionary<TKey, TValue> ToDictionarySafe<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>> enumerable) where TKey : notnull
    {
        return enumerable
            .DistinctBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public static void LoadModule(XmlNode testCase, string baseUrl)
    {
        Console.WriteLine("Loading Module is not implemented.");
    }

    public record Environment(
        XmlNode? ContextNode,
        Func<string, string>? NamespaceResolver,
        Dictionary<string, AbstractValue?>? Variables
    );

    public record TestArguments(
        string BaseUrl,
        XmlNode? ContextNode,
        string TestQuery,
        Language.LanguageId Language,
        Func<string, string?>? NamespaceResolver,
        Dictionary<string, AbstractValue>? VariablesInScope
    )
    {
        public override string ToString()
        {
            return $"BaseUrl: {BaseUrl}\n" +
                   $"ContextNode: {ContextNode}\n" +
                   $"TestQuery: {TestQuery}\n" +
                   $"Language: {Language}\n" +
                   $"NamespaceResolver: {NamespaceResolver}\n" +
                   $"VariablesInScope: {VariablesInScope}";
        }
    }
}