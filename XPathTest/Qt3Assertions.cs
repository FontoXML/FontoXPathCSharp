using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.DomFacade;
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
        var domFacade = new XmlNodeDomFacade();
        var assertNode = Evaluate.EvaluateXPathToFirstNode(
            "./result/*",
            testCase,
            domFacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XmlNode>(namespaceResolver: prefix => "http://www.w3.org/2010/09/qt-fots-catalog"));
        return CreateAsserterForExpression(baseUrl, assertNode, domFacade, language);
    }

    private static AsserterCall CreateAsserterForExpression(
            string baseUrl, XmlNode assertNode, XmlNodeDomFacade domFacade, Language.LanguageId language)
    {
        // TODO: Implement the nodefactory, maybe?
        IDocumentWriter<XmlNode>? nodesFactory = null;

        switch (assertNode.LocalName)
        {
            case "all-of":
            {
                var asserts = Evaluate
                    .EvaluateXPathToNodes("*", assertNode, domFacade)
                    .Select(innerAssertNode =>
                        CreateAsserterForExpression(baseUrl, innerAssertNode, domFacade, language))
                    .ToList();
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    asserts.ForEach(a => a(xpath, contextNode, variablesInScope, namespaceResolver));
                };
            }
            case "any-of":
            {
                var asserts = Evaluate
                    .EvaluateXPathToNodes("*", assertNode, domFacade)
                    .Select(innerAssertNode =>
                        CreateAsserterForExpression(baseUrl, innerAssertNode, domFacade, language))
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
                    var result = Evaluate.EvaluateXPathToString(".", assertNode,
                        domFacade);
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"let $result := ({xpath}) return {result}",
                            contextNode,
                            domFacade,
                            variablesInScope,
                            new Options<XmlNode>(
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
                        Evaluate.EvaluateXPathToBoolean(xpath,
                            contextNode,
                            domFacade,
                            variablesInScope,
                            new Options<XmlNode>(
                                namespaceResolver: namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language)
                        ),
                        $"Expected XPath {xpath} to resolve to true");
                };
            case "assert-eq":
            {
                var equalWith =
                    Evaluate.EvaluateXPathToString(".", assertNode, domFacade);
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            "${xpath} = ${equalWith}",
                            contextNode,
                            domFacade,
                            variablesInScope,
                            new Options<XmlNode>(
                                namespaceResolver: namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language)
                        ),
                        $"Expected XPath {xpath} to resolve to {equalWith}"
                    );
                };
            }
            case "assert-deep-eq":
            {
                var equalWith =
                    Evaluate.EvaluateXPathToString(".", assertNode, domFacade);
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"deep-equal(({xpath}), ({equalWith}))",
                            contextNode,
                            domFacade,
                            variablesInScope,
                            new Options<XmlNode>(
                                namespaceResolver: namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language)
                        ),
                        $"Expected XPath {xpath} to (deep equally) resolve to {equalWith}"
                    );
                };
            }
            case "assert_empty":
            {
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            "(${xpath }) => empty()",
                            contextNode,
                            domFacade,
                            variablesInScope,
                            new Options<XmlNode>(
                                namespaceResolver: namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language)
                        ),
                        $"Expected XPath {xpath} to resolve to the empty sequence");
                };
            }
            case "assert-false":
            {
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.False(
                        Evaluate.EvaluateXPathToBoolean(
                            xpath,
                            contextNode,
                            domFacade,
                            variablesInScope,
                            new Options<XmlNode>(
                                namespaceResolver: namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language)),
                        $"Expected XPath {xpath} to resolve to false"
                    );
                };
            }
            case "assert-count":
            {
                var expectedCount = Evaluate.EvaluateXPathToInt("number(.)",
                    assertNode, domFacade);
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToInt(
                            $"({xpath}) => count()",
                            contextNode,
                            domFacade,
                            variablesInScope,
                            new Options<XmlNode>(
                                namespaceResolver: namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language))
                        == expectedCount, $"Expected {xpath} to resolve to {expectedCount}");
                };
            }
            case "assert-type":
            {
                var expectedType =
                    Evaluate.EvaluateXPathToString(".", assertNode, domFacade);
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"({xpath}) instance of ${expectedType}",
                            contextNode,
                            domFacade,
                            variablesInScope,
                            new Options<XmlNode>(
                                namespaceResolver: namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language)
                        ),
                        $"Expected XPath {xpath} to resolve to something of type {expectedType}"
                    );
                };
            }
            case "assert-xml":
            {
                XmlNode parsedFragment;
                if (Evaluate.EvaluateXPathToBoolean("@file", assertNode, domFacade))
                    parsedFragment = Qt3TestUtils.LoadFileToXmlNode(
                        Evaluate.EvaluateXPathToString($"{baseUrl} || \"/\" || @file",
                            assertNode, domFacade)
                    );
                else
                    parsedFragment = Qt3TestUtils.StringToXmlNode(
                        $"<xml>{Evaluate.EvaluateXPathToString(".", assertNode, domFacade)}</xml>");

                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    var results = Evaluate.EvaluateXPathToNodes(xpath,
                        contextNode, domFacade, variablesInScope,
                        new Options<XmlNode>(
                            namespaceResolver: namespaceResolver,
                            documentWriter: nodesFactory,
                            languageId: language)
                    ).ToList();

                    throw new NotImplementedException("assert-xml not properly implemented yet.");
                };
            }
            case "assert-string-value":
            {
                var expectedString =
                    Evaluate.EvaluateXPathToString(".", assertNode, domFacade);
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToString($"{xpath}",
                            contextNode,
                            domFacade,
                            variablesInScope,
                            new Options<XmlNode>(
                                namespaceResolver: namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language)) == expectedString,
                        $"Expected XPath {xpath} to resolve to {expectedString}"
                    );
                };
            }
            case "error":
            {
                var errorCode = Evaluate.EvaluateXPathToString("@code", assertNode,
                    domFacade);
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    var errorContents = "";
                    Assert.Throws<Exception>(
                        () =>
                        {
                            try
                            {
                                Evaluate.EvaluateXPathToString(xpath,
                                    contextNode,
                                    domFacade,
                                    variablesInScope, new Options<XmlNode>(
                                        namespaceResolver: namespaceResolver,
                                        documentWriter: nodesFactory,
                                        languageId: language));
                            }
                            catch (Exception ex)
                            {
                                errorContents = ex.ToString();
                                throw;
                            }
                        }
                    );
                    Assert.Matches(errorCode == "*" ? ".*" : errorCode, errorContents);
                };
            }
            case "assert-empty":
            {
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean($"({xpath}) => empty()",
                            contextNode,
                            domFacade,
                            variablesInScope,
                            new Options<XmlNode>(
                                namespaceResolver: namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language)
                        ),
                        $"Expected XPath {xpath} to resolve to the empty sequence");
                };
            }
            default:
                return (_, _, _, _) => Assert.True(false, $"Skipped test, it was a {assertNode.LocalName}");
        }
    }
}