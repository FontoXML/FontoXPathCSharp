using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestPathExpressions
{
    private const string TestXml = @"<xml>
        <herp>Herp</herp>
        <derp id=""durp"">derp</derp>
        <derp id=""dorp"">derp</derp>
        <hurr durr=""durrdurrdurr"">durrrrrr</hurr>
    </xml>";

    private static readonly XmlDocument XmlNodeDocument;
    private static readonly XmlNodeDomFacade XmlNodeDomfacade;
    
    private static readonly XDocument XObjectDocument;
    private static readonly XObjectDomFacade XObjectDomfacade;

    static TestPathExpressions()
    {
        XmlNodeDocument = new XmlDocument();
        XmlNodeDocument.LoadXml(TestXml);
        XmlNodeDomfacade = new XmlNodeDomFacade();

        XObjectDocument = XDocument.Parse(TestXml);
        XObjectDomfacade = new XObjectDomFacade();
    }

    private static IEnumerable<XmlNode> XmlNodeEvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            XmlNodeDocument,
            XmlNodeDomfacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XmlNode>(namespaceResolver: _ => null)
        );
    }

    private static string? XmlNodeEvalQueryString(string query)
    {
        return Evaluate.EvaluateXPathToString(
            query,
            XmlNodeDocument,
            XmlNodeDomfacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XmlNode>(namespaceResolver: _ => null)
        );
    }
    
    private static IEnumerable<XObject> XObjectEvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            XObjectDocument,
            XObjectDomfacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XObject>(namespaceResolver: _ => null)
        );
    }

    private static string? XObjectEvalQueryString(string query)
    {
        return Evaluate.EvaluateXPathToString(
            query,
            XObjectDocument,
            XObjectDomfacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XObject>(namespaceResolver: _ => null)
        );
    }

    [Fact]
    public void SimpleAbsolutePath()
    {
        Assert.Single(XmlNodeEvalQueryNodes("/xml/herp"));
        Assert.Single(XObjectEvalQueryNodes("/xml/herp"));
    }

    [Fact]
    public void SimpleRelativePath()
    {
        Assert.Single(XmlNodeEvalQueryNodes("xml/herp"));
        Assert.Single(XObjectEvalQueryNodes("xml/herp"));
    }

    [Fact]
    public void RelativePathEmpty()
    {
        Assert.Empty(XmlNodeEvalQueryNodes("xml/horp"));
        Assert.Empty(XObjectEvalQueryNodes("xml/horp"));
    }

    [Fact]
    public void AbsolutePathEmpty()
    {
        Assert.Empty(XmlNodeEvalQueryNodes("/xml/horp"));
        Assert.Empty(XObjectEvalQueryNodes("/xml/horp"));
    }

    [Fact]
    public void SimpleAttribute()
    {
        Assert.Equal("durrdurrdurr", XmlNodeEvalQueryString("/xml/hurr/@durr"));
        Assert.Equal("durrdurrdurr", XObjectEvalQueryString("/xml/hurr/@durr"));
    }

    [Fact]
    public void AttributeSelect()
    {
        Assert.Single(XmlNodeEvalQueryNodes(@"/xml/derp[@id = ""dorp""]"));
        Assert.Single(XObjectEvalQueryNodes(@"/xml/derp[@id = ""dorp""]"));
    }

    [Fact]
    public void EvalToStringTextContents()
    {
        Assert.Equal("durrrrrr", XmlNodeEvalQueryString("/xml/hurr"));
        Assert.Equal("durrrrrr", XObjectEvalQueryString("/xml/hurr"));
    }
}