using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;
using Xunit.Abstractions;

namespace XPathTest.UnitTests;

public class TestMisc
{
    private static readonly XmlDocument XmlNodeEmptyContext;

    // private static readonly XmlDocument XmlSimpleDocument;
    private static readonly XmlNodeDomFacade XmlNodeDomFacade;
    private static readonly Options<XmlNode> XmlNodeOptions;
    private readonly ITestOutputHelper _testOutputHelper;

    static TestMisc()
    {
        XmlNodeEmptyContext = new XmlDocument();
        // XmlSimpleDocument = new XmlDocument();
        // XmlSimpleDocument.LoadXml("<p />");
        XmlNodeDomFacade = new XmlNodeDomFacade();
        XmlNodeOptions = new Options<XmlNode>(_ => null);
    }

    public TestMisc(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TestFloat()
    {
        Assert.Equal(1,
            Evaluate.EvaluateXPathToInt<string, XmlNode>(
                "xs:float('1')",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    public void TestInstanceOf()
    {
        Assert.False(Evaluate.EvaluateXPathToBoolean<string, XmlNode>(
            "xs:boolean(\"true\") instance of xs:string",
            XmlNodeEmptyContext,
            XmlNodeDomFacade,
            XmlNodeOptions)
        );
    }

    [Fact]
    public void TestPathOrder()
    {
        var document = new XmlDocument();
        document.LoadXml("<x><a/><b/><c/></x>");
        var res = Evaluate
            .EvaluateXPathToNodes("(b,a,c,a)/self::*", document.DocumentElement!, XmlNodeDomFacade, XmlNodeOptions)
            .Select(node => node.Name)
            .ToArray();
        var expected = new[] { "a", "b", "c" };
        Assert.Equal(expected, res);
    }

    [Fact]
    public void TestExpressionCache()
    {
        var selector = string.Concat(Enumerable.Repeat("false() or ", 1000)) + "true()";

        var sw = new Stopwatch();
        sw.Start();
        Evaluate.EvaluateXPathToNodes(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);
        var uncached = sw.Elapsed;
        sw.Restart();
        Evaluate.EvaluateXPathToNodes(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);
        var cached = sw.Elapsed;
        sw.Stop();

        Assert.True(cached < uncached);
    }

    [Fact]
    public void NestedExpression()
    {
        var selector =
            "((((((((((((false() eq false()) eq false()) eq false()) eq " +
            "false()) eq false()) eq false()) eq false()) eq false()) eq " +
            "false()) eq false()) eq false()) eq false()) eq false()";

        var result = Evaluate.EvaluateXPathToBoolean(
            selector,
            XmlNodeEmptyContext,
            XmlNodeDomFacade,
            XmlNodeOptions
        );

        Assert.True(result);
    }


    [Fact]
    public void TextExternalVar()
    {
        var selector = "$x + $y";
        var res = Evaluate.EvaluateXPathToInt(
            selector,
            XmlNodeEmptyContext,
            XmlNodeDomFacade,
            XmlNodeOptions,
            new Dictionary<string, object> { { "x", 1 }, { "y", 2 } }
        );

        Assert.True(res == 3, "Expression should evaluate to 3 (XmlNode)");
    }
}