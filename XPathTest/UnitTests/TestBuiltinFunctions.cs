using System.Collections.Generic;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestBuiltinFunctions
{
    private static string EvalQueryString(string query)
    {
        return Evaluate.EvaluateXPathToString(query, new XmlDocument(), null, new Dictionary<string, AbstractValue>(),
            new Options());
    }

    [Fact]
    public void TestConcatFunction()
    {
        Assert.Equal("test test", EvalQueryString("\"test \" || 'test'"));
    }
}