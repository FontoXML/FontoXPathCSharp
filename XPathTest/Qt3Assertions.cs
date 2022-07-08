using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public delegate void AsserterCall(
    string testQuery,
    XmlNode? contextNode,
    Dictionary<string, AbstractValue> variablesInScope,
    Func<string, string?>? namespaceResolver
);

public class Qt3Assertions
{
    public static AsserterCall GetExpressionBackendAsserterForTest(string baseUrl, XmlNode testCase,
        Language.LanguageId language)
    {
        var assertNode = Evaluate.EvaluateXPathToFirstNode("./result/*", testCase);
        return CreateAsserterForExpression(baseUrl, assertNode, language);
    }

    public static AsserterCall
        CreateAsserterForExpression(
            string baseUrl, XmlNode assertNode, Language.LanguageId language)
    {
        // TODO: Implement the nodefactory, maybe?
        IDocumentWriter? nodesFactory = null;

        switch (assertNode.LocalName)
        {
            case "all-of":
            {
                var asserts = Evaluate.EvaluateXPathToNodes("*", assertNode)
                    .Select(innerAssertNode => CreateAsserterForExpression(baseUrl, innerAssertNode, language))
                    .ToList();
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    asserts.ForEach(a => a(xpath, contextNode, variablesInScope, namespaceResolver));
                };
            }
            case "any-of":
            {
                var asserts = Evaluate.EvaluateXPathToNodes("*", assertNode)
                    .Select(innerAssertNode => CreateAsserterForExpression(baseUrl, innerAssertNode, language))
                    .ToList();
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    var errors = new List<Exception>();

                    Assert.True(asserts.Any(a =>
                        {
                            try
                            {
                                a(xpath, contextNode, variablesInScope, namespaceResolver);
                            }
                            catch (Exception ex)
                            {
                                //TODO: Add TypeError specifically.
                                errors.Add(ex);
                                return false;
                            }

                            return true;
                        }), $"Expected executing the XPath \"{xpath}\" to resolve to one of the expected results, " +
                            $"but got {string.Join(", ", errors.Select(e => e.ToString()))}."
                    );
                };
            }
            case "assert":
            {
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    var result = Evaluate.EvaluateXPathToString('.', assertNode);
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"let $result := ({xpath}) return {result}",
                            contextNode,
                            null,
                            variablesInScope,
                            new Options(
                                namespaceResolver: namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language)
                        ),
                        xpath
                    );
                };
            }
            case "assert-true":
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(xpath, contextNode, null, variablesInScope, new Options(
                            namespaceResolver: namespaceResolver,
                            documentWriter: nodesFactory,
                            languageId: language)
                        ),
                        $"Expected XPath {xpath} to resolve to true");
                };
            case "error":
            case "assert-eq":
            case "assert-deep-eq":
            case "assert_empty":
            case "assert-false":
            case "assert-count":
            case "assert-type":
            case "assert-xml":
            case "assert-string-value":
            {
                throw new NotImplementedException($"Asserter type: {assertNode.LocalName} is not implemented yet.");
            }
            default:
                return (_, _, _, _) => Assert.True(false, $"Skipped test, it was a {assertNode.LocalName}");
        }
    }
}