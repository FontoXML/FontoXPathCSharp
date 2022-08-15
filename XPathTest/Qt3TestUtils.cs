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
    private static readonly Options<XmlNode> Options = new(_ => "http://www.w3.org/2010/09/qt-fots-catalog");

    public static string XmlNodeToString(XmlNode node)
    {
        using var sw = new StringWriter();
        using var xw = new XmlTextWriter(sw);
        xw.Formatting = Formatting.Indented;
        xw.Indentation = 2;

        node.WriteTo(xw);
        return sw.ToString();
    }
    
    public static XmlDocument StringToXmlDocument(string xml)
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

        string testQuery;
        var filename = Evaluate.EvaluateXPathToString("test/@file", testCase, domFacade, Options);
        var filepath = $"{baseUrl}/{filename}";
        
        if (Evaluate.EvaluateXPathToBoolean("./test/@file", testCase, domFacade, Options))
        {
            if (TestFileSystem.FileExists(filepath))
                testQuery = LoadFileToString(filepath);
            else
                throw new FileNotFoundException($"Could not load file {filepath}");
        }
        else
        {
            testQuery = Evaluate.EvaluateXPathToString("./test", testCase, domFacade, Options)!;
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
            Options
        );
        var environmentNodes =
            Evaluate.EvaluateXPathToNodes(
                "./environment",
                testCase,
                domFacade,
                Options
            );
        var environmentNode = environmentNodes.Any()
            ? Evaluate.EvaluateXPathToFirstNode(
                $"/test-set/environment[@name = \"{refString}\"]",
                testCase,
                domFacade,
                Options)
            : Evaluate.EvaluateXPathToFirstNode(
                "./environment",
                testCase,
                domFacade,
                Options);

        var (contextNode, resolver, variablesInScope) = (environmentNode != null
            ? CreateEnvironment(baseUrl, environmentNode, domFacade)
            : EnvironmentsByNameCache.Instance.GetResource(
                Evaluate.EvaluateXPathToString(
                    "(./environment/@ref, \"empty\")[1]",
                    testCase,
                    domFacade,
                    Options) ?? string.Empty
            ))!;

        namespaceResolver = localNamespaceResolver != null
            ? prefix => localNamespaceResolver(prefix) ?? resolver?.Invoke(prefix)
            : prefix => resolver?.Invoke(prefix);

        return new TestArguments(Path.GetDirectoryName(filepath), contextNode, testQuery, language, namespaceResolver, variablesInScope!);
    }

    public static Environment CreateEnvironment(string? baseUrl, XmlNode environmentNode, XmlNodeDomFacade domFacade)
    {
        Func<string, string> nsResolver = prefix => "http://www.w3.org/2010/09/qt-fots-catalog";

        var fileName = Evaluate.EvaluateXPathToString(
            "source[@role=\".\"]/@file",
            environmentNode,
            domFacade,
            Options);

        var variables = Evaluate.EvaluateXPathToNodes(
                "source[@role!=\".\"]",
                environmentNode,
                domFacade,
                Options)
            .Select(variable => new KeyValuePair<string, AbstractValue?>(
                Evaluate.EvaluateXPathToString(
                    "@role",
                    variable,
                    domFacade,
                    Options)?[1..] ?? string.Empty,
                new StringValue(LoadFileToString(
                    (baseUrl != null ? baseUrl + "/" : "") + Evaluate.EvaluateXPathToString(
                        "@file",
                        variable,
                        domFacade,
                        Options)
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