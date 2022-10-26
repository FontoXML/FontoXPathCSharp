using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestKindTests
{
    private readonly XmlNodeDomFacade _xmlNodeDomFacade = new();
    private readonly Options<XmlNode> _xmlNodeOptions = new(_ => null);
    private readonly XmlDocument _xmlNodeDocument = new();

    private readonly XObjectDomFacade _xObjectDomFacade = new();
    private readonly Options<XObject> _xObjectOptions = new(_ => null);
    private readonly XDocument _xObjectDocument = new();
    
    [Fact]
    private void TestSelfDocumentTestFalse()
    {
        const string selector = "self::document-node()";
        
        var element = _xmlNodeDocument.CreateElement("element");
        var res1 = Evaluate.EvaluateXPathToBoolean(selector, element, _xmlNodeDomFacade, _xmlNodeOptions);
        Assert.False(res1, $"(XmlNode) '{selector}' should return false on an element.");
        
        var element2 = new XElement("element");
        var res2 = Evaluate.EvaluateXPathToBoolean(selector, element2, _xObjectDomFacade, _xObjectOptions);
        Assert.False(res2, $"(XObject) '{selector}' should return false on an element.");
    }
    
    [Fact]
    private void TestSelfDocumentTestTrue()
    {
        const string selector = "self::document-node()";
        
        var res1 = Evaluate.EvaluateXPathToBoolean(selector, _xmlNodeDocument, _xmlNodeDomFacade, _xmlNodeOptions);
        Assert.True(res1, $"(XmlNode) '{selector}' should return true on a document node.");
        
        var res2 = Evaluate.EvaluateXPathToBoolean(selector, _xObjectDocument, _xObjectDomFacade, _xObjectOptions);
        Assert.True(res2, $"(XObject) '{selector}' should return true on a document node.");
    }
}