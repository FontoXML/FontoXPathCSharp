using System.Collections.Generic;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestCompare
{
    private const string TestXml = @"<xml>
  <title>xpath.playground.fontoxml.com</title>
  <summary>This is a learning tool for XML, XPath and XQuery.</summary>
  <tips>
    <tip id='edit'>You can edit everything on the left</tip>
    <tip id='examples'>You can access more examples from a menu in the top right</tip>
    <tip id='permalink'>Another button there lets you share your test using an URL</tip>
  </tips>
</xml>";

    private static readonly XmlDocument Document;

    static TestCompare()
    {
        Document = new XmlDocument();
        Document.LoadXml(TestXml);
    }

    private static bool EvalQueryBoolean(string query)
    {
        return Evaluate.EvaluateXPathToBoolean(query, Document, null, new Dictionary<string, AbstractValue>(),
            new Options());
    }

    [Fact]
    public void TestSimpleGeneralCompare()
    {
        Assert.True(EvalQueryBoolean("/xml/title = /xml/title"));
    }
    
    [Fact]
    public void TestSimpleGeneralCompareFalse()
    {
        Assert.False(EvalQueryBoolean("/xml/tips = /xml/title"));
    }
}