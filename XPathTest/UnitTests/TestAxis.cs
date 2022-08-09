using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestAxis
{
    private const string TestXml = @"<xml>
  <title>xpath.playground.fontoxml.com</title>
  <summary>This is a learning tool for XML, XPath and XQuery.</summary>
  <tips>
    <tap></tap>
    <tip id='edit'>You can edit everything on the left</tip>
    <tip id='examples'>You can access more examples from a menu in the top right</tip>
    <tup></tup>
    <tip id='permalink'>Another button there lets you share your test using an URL</tip>
  </tips>
</xml>";

    private static readonly XmlDocument XmlNodeDocument;
    private static readonly XmlNodeDomFacade XmlNodeDomFacade;

    private static readonly XDocument XObjectDocument;
    private static readonly XObjectDomFacade XObjectDomFacade;


    static TestAxis()
    {
        XmlNodeDocument = new XmlDocument();
        XmlNodeDocument.LoadXml(TestXml);
        XmlNodeDomFacade = new XmlNodeDomFacade();

        XObjectDocument = XDocument.Parse(TestXml);
        XObjectDomFacade = new XObjectDomFacade();
    }

    private static IEnumerable<XmlNode> XmlNodeEvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            XmlNodeDocument,
            XmlNodeDomFacade,
            new Options<XmlNode>(_ => null)
        );
    }

    private static IEnumerable<XObject> XObjectEvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            XObjectDocument,
            XObjectDomFacade,
            new Options<XObject>(_ => null)
        );
    }

    [Fact]
    public void TestAncestorAxis()
    {
        Assert.Equal(1, XmlNodeEvalQueryNodes("/xml/tips/tip/ancestor::xml").Count());
        Assert.Equal(1, XObjectEvalQueryNodes("/xml/tips/tip/ancestor::xml").Count());
    }

    [Fact]
    public void TestAncestorAxisSelf()
    {
        Assert.Empty(XmlNodeEvalQueryNodes("/xml/tips/tip/ancestor::tip"));
        Assert.Empty(XObjectEvalQueryNodes("/xml/tips/tip/ancestor::tip"));
    }

    [Fact]
    public void TestDescendantAxis()
    {
        Assert.Equal(3, XmlNodeEvalQueryNodes("descendant::tip").Count());
        Assert.Equal(3, XObjectEvalQueryNodes("descendant::tip").Count());
    }

    [Fact]
    public void TestFollowingAxis()
    {
        Assert.Equal(3, XmlNodeEvalQueryNodes(@"/xml/tips/tap/following::tip").Count());
        Assert.Equal(3, XObjectEvalQueryNodes(@"/xml/tips/tap/following::tip").Count());
    }

    [Fact]
    public void TestPrecedingAxis()
    {
        Assert.Equal(2, XmlNodeEvalQueryNodes(@"/xml/tips/tup/preceding::tip").Count());
        Assert.Equal(2, XObjectEvalQueryNodes(@"/xml/tips/tup/preceding::tip").Count());
    }

    [Fact]
    public void TestFollowingSiblingAxis()
    {
        Assert.Single(XmlNodeEvalQueryNodes(@"/xml/tips/tup/following-sibling::tip"));
        Assert.Single(XObjectEvalQueryNodes(@"/xml/tips/tup/following-sibling::tip"));
    }

    [Fact]
    public void TestPrecedingSiblingAxis()
    {
        Assert.Equal(2, XObjectEvalQueryNodes(@"/xml/tips/tup/preceding-sibling::tip").Count());
        Assert.Equal(2, XmlNodeEvalQueryNodes(@"/xml/tips/tup/preceding-sibling::tip").Count());
    }
}