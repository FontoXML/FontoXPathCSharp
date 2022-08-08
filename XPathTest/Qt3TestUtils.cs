using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
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
        return DocumentsByPathCache.Instance.GetResource($"qt3tests/{PreprocessFilename(filename)}");
    }

    public static TestArguments GetArguments(string testSetFileName, XmlNode testCase, XmlNodeDomFacade domFacade)
    {
        var baseUrl = testSetFileName.Substring(0, testSetFileName.LastIndexOf('/'));

        Func<string, string> nsResolver = (prefix) => "http://www.w3.org/2010/09/qt-fots-catalog";
        
        string testQuery;
        if (Evaluate.EvaluateXPathToBoolean("./test/@file", testCase, domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver)))
        {
            var filename = Evaluate.EvaluateXPathToString("test/@file", testCase, domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver));
            var filepath = $"{baseUrl}/{filename}";
            if (TestFileSystem.FileExists(filepath))
                testQuery = LoadFileToString(filepath);
            else
                throw new FileNotFoundException($"Could not load file {filepath}");
        }
        else
        {
            testQuery = Evaluate.EvaluateXPathToString("./test", testCase, domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver))!;
        }


        //TODO: Retrieve the language from the test case.
        var language = Language.LanguageId.XPATH_3_1_LANGUAGE;

        //TODO: Retrieve namespaces from the test case.
        var namespaces = new Dictionary<string, string>();

        var localNamespaceResolver = namespaces.Count != 0
            ? new Func<string, string?>(prefix => namespaces[prefix])
            : null;

        var namespaceResolver = localNamespaceResolver;

        var refString = Evaluate.EvaluateXPathToString("./environment/@ref",
            testCase, domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver));
        var environmentNodes =
            Evaluate.EvaluateXPathToNodes("./environment", testCase, domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver));
        var environmentNode = environmentNodes.Any()
            ? Evaluate.EvaluateXPathToFirstNode($"/test-set/environment[@name = \"{refString}\"]",
                new NodeValue<XmlNode>(testCase, domFacade), domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver))
            : Evaluate.EvaluateXPathToFirstNode("./environment", new NodeValue<XmlNode>(testCase, domFacade),
                domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver));
        
        var env = environmentNode != null
            ? CreateEnvironment(baseUrl, environmentNode, domFacade)
            : EnvironmentsByNameCache.Instance.GetResource(
                Evaluate.EvaluateXPathToString("(./environment/@ref, \"empty\")[1]", 
                    testCase,
                    domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver)));

        var contextNode = env.ContextNode;
        namespaceResolver = localNamespaceResolver != null
            ? prefix => localNamespaceResolver(prefix) ?? env.NamespaceResolver?.Invoke(prefix)
            : prefix => env.NamespaceResolver?.Invoke(prefix);

        var variablesInScope = env.Variables;

        return new TestArguments(baseUrl, contextNode, testQuery, language, namespaceResolver, variablesInScope!);
    }

    public static Environment CreateEnvironment(string? baseUrl, XmlNode environmentNode, XmlNodeDomFacade domFacade)
    {
        Func<string, string> nsResolver = (prefix) => "http://www.w3.org/2010/09/qt-fots-catalog";
        
        var fileName = Evaluate.EvaluateXPathToString(
            "source[@role=\".\"]/@file",
            environmentNode, 
            domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver));
        
        var variables = Evaluate.EvaluateXPathToNodes("source[@role!=\".\"]",
                environmentNode, domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver))
            .Select(variable => new KeyValuePair<string, AbstractValue?>(
                Evaluate.EvaluateXPathToString("@role", variable, domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver))?[1..],
                new StringValue(LoadFileToString(
                    (baseUrl != null ? baseUrl + "/" : "") + Evaluate.EvaluateXPathToString("@file",
                        variable, domFacade, null, new Options<XmlNode>(namespaceResolver:nsResolver))
                ) ?? string.Empty)))
            .DistinctBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);
        var contextNode = fileName != null && fileName.Length > 0 ? LoadFileToXmlNode(fileName) : null;

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