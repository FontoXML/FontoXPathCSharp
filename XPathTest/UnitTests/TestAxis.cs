using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestAxis
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

    static TestAxis()
    {
        Document = new XmlDocument();
        Document.LoadXml(TestXml);
    }

    private static T EvalQuery<T>(string query)
    {
        var results = Evaluate.EvaluateXPath<T, string>(query, Document, null,
            new Dictionary<string, IExternalValue>(), new Options());
        return results;
    }

    private static IEnumerable<XmlNode> EvalQueryNodes(string query)
    {
        return EvalQuery<IEnumerable<XmlNode>>(query);
    }

    [Fact]
    public void TestAncestorAxis()
    {
        Assert.Single(EvalQueryNodes("/xml/tips/tip/ancestor::xml"));
    }

    [Fact]
    public void TestAncestorAxisSelf()
    {
        Assert.Empty(EvalQueryNodes("/xml/tips/tip/ancestor::tip"));
    }
}