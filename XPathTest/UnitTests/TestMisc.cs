using System;
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
    private readonly ITestOutputHelper _testOutputHelper;
    private static readonly XmlDocument XmlNodeEmptyContext;
    private static readonly XmlNodeDomFacade XmlNodeDomFacade;
    private static readonly Options<XmlNode> XmlNodeOptions;

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
    public void OrderTest()
    {
        var document = new XmlDocument();
        document.LoadXml("<x><a/><b/><c/></x>");
        XmlNode nodeContext = document.DocumentElement!;
        _testOutputHelper.WriteLine(document.OuterXml);
        var res = Evaluate.EvaluateXPathToNodes("(b,a,c,a)/self::*", nodeContext, XmlNodeDomFacade, XmlNodeOptions);
        _testOutputHelper.WriteLine(res.Count().ToString());
        _testOutputHelper.WriteLine($"[{string.Join(", ", res.Select(v => v.Name))}]");
    }

}