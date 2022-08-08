using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using Xunit;

namespace XPathTest.UnitTests;

public class TestMisc
{
    [Fact]
    public void TestFloat()
    {
        Assert.Equal(1, Evaluate.EvaluateXPathToInt<string, XmlNode>("xs:float('1')", null));
    }

    [Fact]
    public void TestInstanceOf()
    {
        Assert.False(
            Evaluate.EvaluateXPathToBoolean<string, XmlNode>("xs:boolean(\"true\") instance of xs:string", null));
    }


    [Fact]
    public void TestDomFacade()
    {
        var domFacade = new XmlNodeDomFacade();
        var xmlDocument = new XmlDocument();
        
        var node = xmlDocument.CreateElement("element");
        Assert.Equal("element" , domFacade.GetLocalName(node));
        Assert.Equal("", domFacade.GetNamespaceUri(node));
        Assert.Equal("", domFacade.GetPrefix(node));
        
        var node2 = xmlDocument.CreateElement("element", "http://www.fontoxml.com/training/");
        Assert.Equal("element" , domFacade.GetLocalName(node2));
        Assert.Equal("http://www.fontoxml.com/training/", domFacade.GetNamespaceUri(node2));
        Assert.Equal("", domFacade.GetPrefix(node2));
        
        var node3 = xmlDocument.CreateElement("prefix","element", "http://www.fontoxml.com/training/");
        Assert.Equal("element" , domFacade.GetLocalName(node3));
        Assert.Equal("http://www.fontoxml.com/training/", domFacade.GetNamespaceUri(node3));
        Assert.Equal("prefix", domFacade.GetPrefix(node3));
    }
    
    [Fact]
    public void TestDomFacadeLinq()
    {
        var domFacade = new XObjectDomFacade();

        XNamespace ns = "http://www.fontoxml.com/training/";

        var node = new XElement("element");
        Assert.Equal("element" , domFacade.GetLocalName(node));
        Assert.Equal("", domFacade.GetNamespaceUri(node));
        Assert.Equal("", domFacade.GetPrefix(node));
        
        var node2 = new XElement(ns + "element");
        Assert.Equal("element" , domFacade.GetLocalName(node2));
        Assert.Equal("http://www.fontoxml.com/training/", domFacade.GetNamespaceUri(node2));
        Assert.Equal("", domFacade.GetPrefix(node2));
        
        var node3 = new XElement(ns + "element", new XAttribute(XNamespace.Xmlns + "prefix", "http://www.fontoxml.com/training/"));
        Assert.Equal("element" , domFacade.GetLocalName(node3));
        Assert.Equal("http://www.fontoxml.com/training/", domFacade.GetNamespaceUri(node3));
        Assert.Equal("prefix", domFacade.GetPrefix(node3));
    }
}