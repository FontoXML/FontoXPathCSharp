using System;
using System.Collections.Generic;
using System.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.Qt3Tests;

public delegate void AsserterCall<TNode>(
    string testQuery,
    TNode contextNode,
    Dictionary<string, object> variablesInScope,
    NamespaceResolver namespaceResolver
) where TNode : notnull;

public class Qt3Assertions<TNode> where TNode : notnull
{
    private static readonly Options<TNode> Qt3Options;

    static Qt3Assertions()
    {
        Qt3Options = new Options<TNode>(
            _ => "http://www.w3.org/2010/09/qt-fots-catalog"
        );
    }

    public static AsserterCall<TNode> GetExpressionBackendAsserterForTest(string baseUrl, TNode testCase,
        Language.LanguageId language, IDomFacade<TNode> domFacade, INodeUtils<TNode> nodeUtils)
    {
        var assertNode = Evaluate.EvaluateXPathToFirstNode(
            "./result/*",
            testCase,
            domFacade,
            Qt3Options);
        return CreateAsserterForExpression(baseUrl, assertNode!, domFacade, language, nodeUtils);
    }

    private static AsserterCall<TNode> CreateAsserterForExpression(
        string baseUrl,
        TNode assertNode,
        IDomFacade<TNode> domFacade,
        Language.LanguageId language,
        INodeUtils<TNode> nodeUtils)
    {
        // TODO: Implement the nodefactory, maybe?
        IDocumentWriter<TNode>? nodesFactory = null;

        // if (assertNode.LocalName != "assert-xml")  return (_, _, _, _) => Assert.True(false, $"Skipped test, it was a {assertNode.LocalName}");

        switch (domFacade.GetLocalName(assertNode))
        {
            case "all-of":
            {
                var asserts = Evaluate
                    .EvaluateXPathToNodes(
                        "*",
                        assertNode,
                        domFacade,
                        Qt3Options)
                    .Select(innerAssertNode =>
                        CreateAsserterForExpression(baseUrl, innerAssertNode, domFacade, language, nodeUtils))
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
                        Qt3Options)
                    .Select(innerAssertNode =>
                        CreateAsserterForExpression(baseUrl, innerAssertNode, domFacade, language, nodeUtils))
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
                        }), $"Expected executing the XPath '{SingleLinePrint(xpath)}' to resolve to one of the expected results, " +
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
                        Qt3Options);
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"let $result := ({xpath}) return {result}",
                            contextNode,
                            domFacade,
                            new Options<TNode>(
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
                            new Options<TNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
                        ),
                        $"Expected XPath {SingleLinePrint(xpath)} to resolve to true");
                };
            case "assert-eq":
            {
                var equalWith =
                    Evaluate.EvaluateXPathToString(
                        ".",
                        assertNode,
                        domFacade,
                        Qt3Options
                    );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"{xpath} = {equalWith}",
                            contextNode,
                            domFacade,
                            new Options<TNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
                        ),
                        $"Expected XPath {SingleLinePrint(xpath)} to resolve to {equalWith}"
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
                        Qt3Options
                    );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"deep-equal(({xpath}), ({equalWith}))",
                            contextNode,
                            domFacade,
                            new Options<TNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
                        ),
                        $"Expected XPath {SingleLinePrint(xpath)} to (deep equally) resolve to {equalWith}"
                    );
                };
            }
            case "assert_empty":
            {
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"({xpath}) => empty()",
                            contextNode,
                            domFacade,
                            new Options<TNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
                        ),
                        $"Expected XPath {SingleLinePrint(xpath)} to resolve to the empty sequence");
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
                            new Options<TNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope),
                        $"Expected XPath {SingleLinePrint(xpath)} to resolve to false"
                    );
                };
            }
            case "assert-count":
            {
                var expectedCount = Evaluate.EvaluateXPathToInt(
                    "number(.)",
                    assertNode,
                    domFacade,
                    Qt3Options
                );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToInt(
                            $"({xpath}) => count()",
                            contextNode,
                            domFacade,
                            new Options<TNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope)
                        == expectedCount, $"Expected {SingleLinePrint(xpath)} to resolve to {expectedCount}");
                };
            }
            case "assert-type":
            {
                var expectedType =
                    Evaluate.EvaluateXPathToString(
                        ".",
                        assertNode,
                        domFacade,
                        Qt3Options
                    );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    Assert.True(
                        Evaluate.EvaluateXPathToBoolean(
                            $"({xpath}) instance of ${expectedType}",
                            contextNode,
                            domFacade,
                            new Options<TNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
                        ),
                        $"Expected XPath {SingleLinePrint(xpath)} to resolve to something of type {expectedType}"
                    );
                };
            }
            case "assert-xml":
            {
                TNode parsedFragment;
                if (Evaluate.EvaluateXPathToBoolean(
                        "@file",
                        assertNode,
                        domFacade,
                        Qt3Options))
                    parsedFragment = nodeUtils.LoadFileToXmlNode(
                        Evaluate.EvaluateXPathToString(
                            $"{baseUrl} || \"/\" || @file",
                            assertNode,
                            domFacade,
                            Qt3Options)!
                    )!;
                else
                    parsedFragment = domFacade.GetDocumentElement(
                        nodeUtils.StringToXmlDocument(
                            $"<xml>{Evaluate.EvaluateXPathToString(".", assertNode, domFacade, Qt3Options)}</xml>"
                        )
                    )!;

                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    var results = Evaluate.EvaluateXPathToNodes(
                        xpath,
                        contextNode,
                        domFacade,
                        new Options<TNode>(
                            namespaceResolver,
                            documentWriter: nodesFactory,
                            languageId: language),
                        variablesInScope
                    ).ToList();


                    var parsedFragmentChildren = domFacade
                        .GetChildNodes(parsedFragment)
                        .ToList();

                    Assert.True(parsedFragmentChildren.Count == results.Count,
                        "Expected results and parsedFragment children to match");


                    for (var i = 0; i < results.Count; i++)
                        Assert.True(nodeUtils.NodeToString(results[i]) ==
                                    nodeUtils.NodeToString(parsedFragmentChildren[i]),
                            "Expected all children to match between result and parsedFragment.");
                };
            }
            case "assert-string-value":
            {
                var expectedString =
                    Evaluate.EvaluateXPathToString(
                        ".",
                        assertNode,
                        domFacade,
                        Qt3Options
                    );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    var result = Evaluate.EvaluateXPathToString(xpath,
                        contextNode,
                        domFacade,
                        new Options<TNode>(
                            namespaceResolver,
                            documentWriter: nodesFactory,
                            languageId: language),
                        variablesInScope);
                    Assert.True(
                        result == expectedString,
                        $"Expected XPath {SingleLinePrint(xpath)} to resolve to {expectedString}, but instead got {result ?? "null"}"
                    );
                };
            }
            case "error":
            {
                var errorCode = Evaluate.EvaluateXPathToString(
                    "@code",
                    assertNode,
                    domFacade,
                    Qt3Options
                );
                return (xpath, contextNode, variablesInScope, namespaceResolver) =>
                {
                    var actualErrorCode = "";
                    var errorMessage = "";
                    string? result = null;
                    try
                    {
                        result = Evaluate.EvaluateXPathToString(xpath,
                            contextNode,
                            domFacade,
                            new Options<TNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope);
                    }
                    catch (XPathException ex)
                    {
                        actualErrorCode = ex.ErrorCode;
                        errorMessage = ex.Message.Split(Environment.NewLine).First();
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.Message.Split(Environment.NewLine).First();
                        Assert.True(false, $"Expected XPathException with code {errorCode}, but got {errorMessage}");
                    }

                    if (result == null)
                        Assert.True(
                            errorCode == actualErrorCode,
                            $"Expected error code: {errorCode}. Found error code: {actualErrorCode} with error message: {errorMessage}"
                        );
                    Assert.True(
                        errorCode == actualErrorCode,
                        $"Expected error code: {errorCode}. Query succeeded with result: {result}"
                    );
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
                            new Options<TNode>(
                                namespaceResolver,
                                documentWriter: nodesFactory,
                                languageId: language),
                            variablesInScope
                        ),
                        $"Expected XPath {SingleLinePrint(xpath)} to resolve to the empty sequence");
                };
            }
            default:
                return (_, _, _, _) =>
                    Assert.True(false, $"Skipped test, it was a {domFacade.GetLocalName(assertNode)}");
        }
    }

    private static string SingleLinePrint(string input)
    {
        return input.Trim().ReplaceLineEndings().Replace(Environment.NewLine, "\\n").Replace("\t", "\\t");
    }
}