using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
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
    Dictionary<string, object> variablesInScope,
    Func<string, string?>? namespaceResolver
);

public class Qt3Assertions
{
    private static readonly Options<XmlNode> Qt3XmlNodeOptions;

    static Qt3Assertions()
    {
        Qt3XmlNodeOptions = new Options<XmlNode>(
            _ => "http://www.w3.org/2010/09/qt-fots-catalog"
        );
    }

    public static AsserterCall GetExpressionBackendAsserterForTest(string baseUrl, XmlNode testCase,
        Language.LanguageId language)
    {
        var domFacade = new XmlNodeDomFacade();
        var assertNode = Evaluate.EvaluateXPathToFirstNode(
            "./result/*",
            testCase,
            domFacade,
            Qt3XmlNodeOptions);
        return CreateAsserterForExpression(baseUrl, assertNode, domFacade, language);
    }

    private static AsserterCall CreateAsserterForExpression(
        string baseUrl, XmlNode assertNode, XmlNodeDomFacade domFacade, Language.LanguageId language)
    {
        // TODO: Implement the nodefactory, maybe?
        IDocumentWriter<XmlNode>? nodesFactory = null;

        // if (assertNode.LocalName != "assert-xml")  return (_, _, _, _) => Assert.True(false, $"Skipped test, it was a {assertNode.LocalName}");
        
        switch (assertNode.LocalName)
        {
            case "all-of":
            {
                var asserts = Evaluate
                    .EvaluateXPathToNodes(
                        "*",
                        assertNode,
                        domFacade,
                        Qt3XmlNodeOptions)
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
                    .EvaluateXPathToNodes(
                        "*",
                        assertNode,
                        domFacade,
                        Qt3XmlNodeOptions)
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
                    var result = Evaluate.EvaluateXPathToString(
                        ".",
                        assertNode,
                        domFacade,
                        Qt3XmlNodeOptions);
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"let $result := ({xpath}) return {result}",
                            contextNode,
                            domFacade,
                            new Options<XmlNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
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
                            new Options<XmlNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
                        ),
                        $"Expected XPath {xpath} to resolve to true");
                };
            case "assert-eq":
            {
                var equalWith =
                    Evaluate.EvaluateXPathToString(
                        ".",
                        assertNode,
                        domFacade,
                        Qt3XmlNodeOptions
                    );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            "${xpath} = ${equalWith}",
                            contextNode,
                            domFacade,
                            new Options<XmlNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
                        ),
                        $"Expected XPath {xpath} to resolve to {equalWith}"
                    );
                };
            }
            case "assert-deep-eq":
            {
                var equalWith =
                    Evaluate.EvaluateXPathToString(
                        ".",
                        assertNode,
                        domFacade,
                        Qt3XmlNodeOptions
                    );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"deep-equal(({xpath}), ({equalWith}))",
                            contextNode,
                            domFacade,
                            new Options<XmlNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
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
                            new Options<XmlNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
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
                            new Options<XmlNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope),
                        $"Expected XPath {xpath} to resolve to false"
                    );
                };
            }
            case "assert-count":
            {
                var expectedCount = Evaluate.EvaluateXPathToInt(
                    "number(.)",
                    assertNode,
                    domFacade,
                    Qt3XmlNodeOptions
                );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToInt(
                            $"({xpath}) => count()",
                            contextNode,
                            domFacade,
                            new Options<XmlNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope)
                        == expectedCount, $"Expected {xpath} to resolve to {expectedCount}");
                };
            }
            case "assert-type":
            {
                var expectedType =
                    Evaluate.EvaluateXPathToString(
                        ".",
                        assertNode,
                        domFacade,
                        Qt3XmlNodeOptions
                    );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"({xpath}) instance of ${expectedType}",
                            contextNode,
                            domFacade,
                            new Options<XmlNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
                        ),
                        $"Expected XPath {xpath} to resolve to something of type {expectedType}"
                    );
                };
            }
            case "assert-xml":
            {
                XmlNode? parsedFragment;
                if (Evaluate.EvaluateXPathToBoolean(
                        "@file",
                        assertNode,
                        domFacade,
                        Qt3XmlNodeOptions))
                {
                    parsedFragment = Qt3TestUtils.LoadFileToXmlNode(
                        Evaluate.EvaluateXPathToString(
                            $"{baseUrl} || \"/\" || @file",
                            assertNode,
                            domFacade,
                            Qt3XmlNodeOptions)
                    );
                }
                else
                {
                    parsedFragment = Qt3TestUtils.StringToXmlDocument(
                            $"<xml>{Evaluate.EvaluateXPathToString(".", assertNode, domFacade, Qt3XmlNodeOptions)}</xml>")
                        .DocumentElement;
                }

                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    var results = Evaluate.EvaluateXPathToNodes(
                        xpath,
                        contextNode,
                        domFacade,
                        new Options<XmlNode>(
                            namespaceResolver,
                            documentWriter: nodesFactory,
                            languageId: language),
                        variablesInScope
                    ).ToList();

                    var parsedFragmentChildren = parsedFragment
                        .ChildNodes
                        .Cast<XmlNode>()
                        .ToList();

                    Assert.True(parsedFragmentChildren.Count == results.Count,
                        "Expected results and parsedFragment children to match");


                    for (var i = 0; i < results.Count; i++)
                    {
                        Assert.True(Qt3TestUtils.XmlNodeToString(results[i]) ==
                                    Qt3TestUtils.XmlNodeToString(parsedFragmentChildren[i]),
                            "Expected all children to match between result and parsedFragment.");
                    }
                };
            }
            case "assert-string-value":
            {
                var expectedString =
                    Evaluate.EvaluateXPathToString(
                        ".",
                        assertNode,
                        domFacade,
                        Qt3XmlNodeOptions
                    );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToString($"{xpath}",
                            contextNode,
                            domFacade,
                            new Options<XmlNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope) == expectedString,
                        $"Expected XPath {xpath} to resolve to {expectedString}"
                    );
                };
            }
            case "error":
            {
                var errorCode = Evaluate.EvaluateXPathToString(
                    "@code",
                    assertNode,
                    domFacade,
                    Qt3XmlNodeOptions
                );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    var errorContents = "";
                    var actualErrorCode = "";
                    Assert.Throws<XPathException>(
                        () =>
                        {
                            try
                            {
                                Evaluate.EvaluateXPathToString(xpath,
                                    contextNode,
                                    domFacade,
                                    new Options<XmlNode>(
                                        namespaceResolver,
                                        documentWriter: nodesFactory,
                                        languageId: language),
                                    variablesInScope);
                            }
                            catch (XPathException ex)
                            {
                                actualErrorCode = ex.ErrorCode;
                                errorContents = ex.Message;
                                throw;
                            }
                        }
                    );
                    Assert.Equal(errorCode, actualErrorCode);
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
                            new Options<XmlNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
                        ),
                        $"Expected XPath {xpath} to resolve to the empty sequence");
                };
            }
            default:
                return (_, _, _, _) => Assert.True(false, $"Skipped test, it was a {assertNode.LocalName}");
        }
    }
}