using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest;

public class QT3Tests
{
    [Theory(Skip = "Not implemented completely")]
    [ClassData(typeof(Qt3TestDataProvider))]
    [Description]
    public void Qt3Tests(TestCase testCase)
    {
        // var arguments = GetArguments(testCase.TestSetFileName, testCase);
        //
        // try
        // {
        //     loadModule(testCase, baseUrl);
        //
        //     const asserter  = getExpressionBackendAsserterForTest(
        //         baseUrl,
        //         testCase,
        //         language,
        //         annotateAst
        //     );
        //
        //     asserter(
        //         testQuery,
        //         contextNode,
        //         variablesInScope,
        //         namespaceResolver
        //     );
        // }
        // catch (e)
        // {
        //     if (e instanceof TypeError) {
        //         throw e;
        //     }
        //
        //     expressionBackendLog.push(
        //         `${
        //         name
        //     }, ${
        //         e.toString().replace( /\n / g, ' ').trim()
        //     }`
        //     );
        //
        //     // And rethrow the error
        //     throw e;
        // }
    }


    private static TestArguments GetArguments(string testSetFileName, XmlNode testCase)
    {
        var baseUrl = testSetFileName.Substring(0, testSetFileName.LastIndexOf('/'));

        string? testQuery;
        if (Evaluate.EvaluateXPathToBoolean("./test/@file", testCase, null, new Dictionary<string, AbstractValue>(),
                new Options()))
        {
            testQuery = Qt3TestUtils.LoadFileToString(
                Evaluate.EvaluateXPathToString(
                    @"$baseUrl || ""/"" || test/@file",
                    testCase,
                    null,
                    new() {{"baseUrl", new StringValue(baseUrl)}},
                    new Options())
            );
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
            ? new Func<string, string>(prefix => namespaces[prefix])
            : null;

        var namespaceResolver = localNamespaceResolver;

        XmlDocument environmentNode = null;

        // var env = environmentNode != null
        //     ? CreateEnvironment(baseUrl, environmentNode)
        //     : EnvironmentsByName[evaluateXPathToString('(./environment/@ref, "empty")[1]', testCase)];
        //
        // var contextNode = env.contextNode;
        // namespaceResolver = localNamespaceResolver
        //     ? (prefix) => localNamespaceResolver(prefix) || env.namespaceResolver(prefix)
        //     : (prefix) =>
        //         env.namespaceResolver
        //             ? env.namespaceResolver(prefix)
        //             : prefix === ''
        //                 ? null
        //                 : undefined;
        // variablesInScope = env.variables;
        //
        // return new TestArguments(baseUrl, )
        return new TestArguments(baseUrl, environmentNode, "", Language.LanguageId.XPATH_3_1_LANGUAGE, s => s,
            new Dictionary<string, string>());
    }
}

public record TestCase(string Name, string Description, bool Skip, string TestSetFileName, TestArguments Arguments);

public record TestArguments(
    string BaseUrl,
    XmlNode ContextNode,
    string TestQuery,
    Language.LanguageId Language,
    Func<string, string> NamespaceResolver,
    Dictionary<string, string> VariablesInScope
);