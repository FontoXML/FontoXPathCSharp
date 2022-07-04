using System.Collections.Generic;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestPathExpressions
{
    private const string TestXml = @"<xml>
        <herp>Herp</herp>
        <derp id=""durp"">derp</derp>
        <hurr durr=""durrdurrdurr"">durrrrrr</hurr>
    </xml>";

    private static readonly XmlDocument Document;

    static TestPathExpressions()
    {
        Document = new XmlDocument();
        Document.LoadXml(TestXml);
    }

    private static IEnumerable<XmlNode> EvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(query, Document, null, new Dictionary<string, IExternalValue>(),
            new Options());
    }

    private static string EvalQueryString(string query)
    {
        return Evaluate.EvaluateXPathToString(query, Document, null, new Dictionary<string, IExternalValue>(),
            new Options());
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
        Assert.Single(EvalQueryNodes("/xml/derp/@id"));
    }
}