using System.Collections.Generic;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestBuiltinFunctions
{
    private static T EvalQuery<T>(string query)
    {
        return Evaluate.EvaluateXPath<T, string>(query, new XmlDocument(), null,
            new Dictionary<string, AbstractValue>(), new Options());
    }

    private static IEnumerable<XmlNode> EvalQueryNodes(string query)
    {
        return EvalQuery<IEnumerable<XmlNode>>(query);
    }

    private static string EvalQueryString(string query)
    {
        return EvalQuery<string>(query);
    }

    [Fact]
    public void TestConcatFunction()
    {
        Assert.Equal("test test", EvalQueryString("\"test \" || 'test'"));
    }
}