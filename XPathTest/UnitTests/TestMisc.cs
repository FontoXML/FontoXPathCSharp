using System;
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
    private static readonly XmlNodeDomFacade XmlNodeDomFacade;
    private static readonly Options<XmlNode> XmlNodeOptions;
    private readonly ITestOutputHelper _testOutputHelper;

    static TestMisc()
    {
        XmlNodeEmptyContext = new XmlDocument();
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
        var expected = new[] { "a", "a", "b", "c" };
        Assert.Equal(res, expected);
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
        
        _testOutputHelper.WriteLine($"Uncached: {uncached.TotalSeconds}s, Cached: {cached.TotalSeconds}s");
        Assert.True(cached < uncached);
    }
}