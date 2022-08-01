using System.Collections.Generic;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestBuiltinFunctions
{
    private static readonly XmlDocument Document;
    private static readonly XmlNodeDomFacade Domfacade;

    static TestBuiltinFunctions()
    {
        Document = new XmlDocument();
        Domfacade = new XmlNodeDomFacade();
    }

    private static string? EvalQueryString(string query)
    {
        return Evaluate.EvaluateXPathToString(
            query,
            new NodeValue<XmlNode>(Document, Domfacade),
            Domfacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XmlNode>()
        );
    }

    [Fact]
    public void TestConcatFunction()
    {
        Assert.Equal("test test", EvalQueryString("\"test \" || 'test'"));
    }
}