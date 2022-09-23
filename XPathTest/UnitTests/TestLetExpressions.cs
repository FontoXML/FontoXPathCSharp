using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestLetExpressions
{
    private static readonly XmlDocument XmlNodeDocument;
    private static readonly XmlNodeDomFacade XmlNodeDomfacade;
    private static readonly Options<XmlNode> XmlNodeOptions = new(_ => "");

    private static readonly XDocument XObjectDocument;
    private static readonly XObjectDomFacade XObjectDomFacade;
    private static readonly Options<XObject> XObjectOptions = new(_ => "");

    static TestLetExpressions()
    {
        XmlNodeDocument = new XmlDocument();
        XmlNodeDomfacade = new XmlNodeDomFacade();

        XObjectDocument = new XDocument();
        XObjectDomFacade = new XObjectDomFacade();
    }


    [Fact]
    public void LetExpressionTest()
    {
        var selector = "let $x := 1 return $x";
        var res = Evaluate.EvaluateXPathToInt(
            selector,
            XmlNodeDocument,
            XmlNodeDomfacade,
            XmlNodeOptions
        );

        Assert.True(res == 1, "Expression should evaluate to 1 (XmlNode)");

        var res2 = Evaluate.EvaluateXPathToInt(
            selector,
            XObjectDocument,
            XObjectDomFacade,
            XObjectOptions
        );

        Assert.True(res2 == 1, "Expression should evaluate to 1 (XObject)");
    }

    [Fact]
    public void LetExpressionTest2()
    {
        var selector = "let $x := 1, let $y := 2 return $x + $y";
        var res = Evaluate.EvaluateXPathToInt(
            selector,
            XmlNodeDocument,
            XmlNodeDomfacade,
            XmlNodeOptions
        );

        Assert.True(res == 3, "Expression should evaluate to 3 (XmlNode)");

        var res2 = Evaluate.EvaluateXPathToInt(
            selector,
            XObjectDocument,
            XObjectDomFacade,
            XObjectOptions
        );

        Assert.True(res2 == 3, "Expression should evaluate to 3 (XObject)");
    }

    [Fact]
    public void LetExpressionTest3()
    {
        var selector = "let $x := 1, " +
                       "let $x := $x + 1 " +
                       "return $x";

        var res = Evaluate.EvaluateXPathToInt(
            selector,
            XmlNodeDocument,
            XmlNodeDomfacade,
            XmlNodeOptions
        );

        Assert.True(res == 2, "Expression should evaluate to 2 (XmlNode)");

        var res2 = Evaluate.EvaluateXPathToInt(
            selector,
            XObjectDocument,
            XObjectDomFacade,
            XObjectOptions
        );

        Assert.True(res2 == 2, "Expression should evaluate to 2 (XObject)");
    }
}