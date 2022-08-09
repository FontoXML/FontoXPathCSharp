using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestAttributeElementTest
{
    private const string TestXml = @"<xml xmlns=""blabla"">
  <title>xpath.playground.fontoxml.com</title>
  <summary>This is a learning tool for XML, XPath and XQuery.</summary>
  <tips>
    <tap></tap>
    <tip id='edit'>You can edit everything on the left</tip>
    <tip id='examples'>You can access more examples from a menu in the top right</tip>
    <tup></tup>
    <tip id='permalink' attr='test'>Another button there lets you share your test using an URL</tip>
  </tips>
</xml>";

    private static readonly XmlDocument XmlNodeDocument;
    private static readonly XmlNodeDomFacade XmlNodeDomfacade;

    private static readonly XDocument XObjectDocument;
    private static readonly XObjectDomFacade XObjectDomFacade;

    static TestAttributeElementTest()
    {
        XmlNodeDocument = new XmlDocument();
        XmlNodeDocument.LoadXml(TestXml);
        XmlNodeDomfacade = new XmlNodeDomFacade();

        XObjectDocument = XDocument.Parse(TestXml);
        XObjectDomFacade = new XObjectDomFacade();
    }

    private static IEnumerable<XmlNode> EvalQueryNodesXmlNode(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            XmlNodeDocument,
            XmlNodeDomfacade,
            new Options<XmlNode>(_ => "blabla")
        );
    }

    private static IEnumerable<XObject> EvalQueryNodesXObject(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            XObjectDocument,
            XObjectDomFacade,
            new Options<XObject>(_ => "blabla")
        );
    }

    [Fact]
    public void TestEmptyAttribute()
    {
        Assert.Equal(4, EvalQueryNodesXmlNode("/xml/tips/tip/attribute()").Count());
        Assert.Equal(4, EvalQueryNodesXObject("/xml/tips/tip/attribute()").Count());
    }

    [Fact]
    public void TestNamedAttribute()
    {
        Assert.Equal(3, EvalQueryNodesXmlNode("/xml/tips/tip/attribute(id)").Count());
        Assert.Equal(3, EvalQueryNodesXObject("/xml/tips/tip/attribute(id)").Count());
    }

    [Fact]
    public void TestNamedAttribute2()
    {
        Assert.Single(EvalQueryNodesXmlNode("/xml/tips/tip[@id = 'edit']"));
        Assert.Single(EvalQueryNodesXObject("/xml/tips/tip[@id = 'edit']"));
    }


    [Fact]
    public void TestEmptyElement()
    {
        Assert.Equal(5, EvalQueryNodesXmlNode("/xml/tips/element()").Count());
        Assert.Equal(5, EvalQueryNodesXObject("/xml/tips/element()").Count());
    }

    [Fact]
    public void TestNamedElement()
    {
        Assert.Equal(3, EvalQueryNodesXmlNode("/xml/tips/element(tip)").Count());
        Assert.Equal(3, EvalQueryNodesXObject("/xml/tips/element(tip)").Count());
    }

    [Fact]
    public void TestWildcard()
    {
        Assert.Equal(5, EvalQueryNodesXmlNode("/xml/tips/*").Count());
        Assert.Equal(5, EvalQueryNodesXObject("/xml/tips/*").Count());
    }

    [Fact]
    public void TestNameTest()
    {
        Assert.Single(EvalQueryNodesXmlNode("/xml"));
        Assert.Single(EvalQueryNodesXObject("/xml"));
    }
}