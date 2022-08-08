using System.Collections.Generic;
using System.Xml;
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

    private static readonly XmlDocument Document;
    private static readonly XmlNodeDomFacade Domfacade;

    static TestPathExpressions()
    {
        Document = new XmlDocument();
        Document.LoadXml(TestXml);
        Domfacade = new XmlNodeDomFacade();
    }

    private static IEnumerable<XmlNode> EvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            Document,
            Domfacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XmlNode>(namespaceResolver: _ => null)
        );
    }

    private static string? EvalQueryString(string query)
    {
        return Evaluate.EvaluateXPathToString(
            query,
            Document,
            Domfacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XmlNode>(namespaceResolver: _ => null)
        );
    }

    [Fact]
    public void SimpleAbsolutePath()
    {
        Assert.Single(EvalQueryNodes("/xml/herp"));
    }

    [Fact]
    public void SimpleRelativePath()
    {
        Assert.Single(EvalQueryNodes("xml/herp"));
    }

    [Fact]
    public void RelativePathEmpty()
    {
        Assert.Empty(EvalQueryNodes("xml/horp"));
    }

    [Fact]
    public void AbsolutePathEmpty()
    {
        Assert.Empty(EvalQueryNodes("/xml/horp"));
    }

    [Fact]
    public void SimpleAttribute()
    {
        Assert.Equal("durrdurrdurr", EvalQueryString("/xml/hurr/@durr"));
    }

    [Fact]
    public void AttributeSelect()
    {
        Assert.Single(EvalQueryNodes(@"/xml/derp[@id = ""dorp""]"));
    }

    [Fact]
    public void EvalToStringTextContents()
    {
        Assert.Equal("durrrrrr", EvalQueryString("/xml/hurr"));
    }
}