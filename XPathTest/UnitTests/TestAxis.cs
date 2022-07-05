using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
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

    private static readonly XmlDocument Document;

    static TestAxis()
    {
        Document = new XmlDocument();
        Document.LoadXml(TestXml);
    }

    private static T EvalQuery<T>(string query)
    {
        var results = Evaluate.EvaluateXPath<T, string>(query, Document, null,
            new Dictionary<string, AbstractValue>(), new Options());
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

    [Fact]
    public void TestDescendantAxis()
    {
        Assert.Equal(3, EvalQueryNodes("descendant::tip").Count());
    }

    [Fact]
    public void TestFollowingAxis()
    {
        Assert.Equal(3, EvalQueryNodes(@"/xml/tips/tap/following::tip").Count());
    }

    [Fact]
    public void TestPrecedingAxis()
    {
        Assert.Equal(2, EvalQueryNodes(@"/xml/tips/tup/preceding::tip").Count());
    }

    [Fact]
    public void TestFollowingSiblingAxis()
    {
        Assert.Single(EvalQueryNodes(@"/xml/tips/tup/following-sibling::tip"));
    }
}