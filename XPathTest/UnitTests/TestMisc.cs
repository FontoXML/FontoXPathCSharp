using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestMisc
{
    private static readonly XmlDocument XmlNodeEmptyContext;
    private static readonly XmlNodeDomFacade XmlNodeDomFacade;
    private static readonly Options<XmlNode> XmlNodeOptions;

    static TestMisc()
    {
        XmlNodeEmptyContext = new XmlDocument();
        XmlNodeDomFacade = new XmlNodeDomFacade();
        XmlNodeOptions = new Options<XmlNode>(_ => null);
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
}