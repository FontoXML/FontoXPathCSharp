using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestBuiltinFunctions
{
    private static readonly XmlDocument XmlNodeDocument;
    private static readonly XmlNodeDomFacade XmlNodeDomfacade;

    private static readonly XDocument XObjectDocument;
    private static readonly XObjectDomFacade XObjectDomFacade;

    static TestBuiltinFunctions()
    {
        XmlNodeDocument = new XmlDocument();
        XmlNodeDomfacade = new XmlNodeDomFacade();

        XObjectDocument = new XDocument();
        XObjectDomFacade = new XObjectDomFacade();
    }

    private static string? XmlNodeEvalQueryString(string query)
    {
        return Evaluate.EvaluateXPathToString(
            query,
            XmlNodeDocument,
            XmlNodeDomfacade,
            new Options<XmlNode>(_ => null)
        );
    }

    private static string? XObjectEvalQueryString(string query)
    {
        return Evaluate.EvaluateXPathToString(
            query,
            XObjectDocument,
            XObjectDomFacade,
            new Options<XObject>(_ => null)
        );
    }


    [Fact]
    public void TestConcatFunction()
    {
        Assert.Equal("test test", XmlNodeEvalQueryString("\"test \" || 'test'"));
        Assert.Equal("test test", XObjectEvalQueryString("\"test \" || 'test'"));
    }
}