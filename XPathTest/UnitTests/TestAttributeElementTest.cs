using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestAttributeElementTest
{
    private const string TestXml = @"<xml>
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

    private static readonly XmlDocument Document;

    static TestAttributeElementTest()
    {
        Document = new XmlDocument();
        Document.LoadXml(TestXml);
    }

    private static IEnumerable<XmlNode> EvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(query, Document, null, new Dictionary<string, AbstractValue>(),
            new Options());
    }

    [Fact]
    public void TestEmptyAttribute()
    {
      Assert.Equal(4, EvalQueryNodes("/xml/tips/tip/attribute()").Count());
    }
    
    [Fact]
    public void TestNamedAttribute()
    {
      Assert.Equal(3, EvalQueryNodes("/xml/tips/tip/attribute(id)").Count());
    }
    
    [Fact]
    public void TestEmptyElement()
    {
      Assert.Equal(5, EvalQueryNodes("/xml/tips/element()").Count());
    }
    
    [Fact]
    public void TestNamedElement()
    {
      Assert.Equal(3, EvalQueryNodes("/xml/tips/element(tip)").Count());
    }
}