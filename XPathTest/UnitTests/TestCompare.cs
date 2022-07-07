using System.Collections.Generic;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestCompare
{
    private static bool EvalQueryBoolean(string query)
    {
        return Evaluate.EvaluateXPathToBoolean(query, new XmlDocument(), null, new Dictionary<string, AbstractValue>(),
            new Options());
    }

    [Fact]
    public void TestValueCompareIntEqual()
    {
        Assert.True(EvalQueryBoolean("12 eq 12"));
    }

    [Fact]
    public void TestValueCompareIntEqualFalse()
    {
        Assert.False(EvalQueryBoolean("12 eq 13"));
    }
    
    [Fact]
    public void TestValueCompareIntNotEqual()
    {
        Assert.True(EvalQueryBoolean("12 ne 13"));
    }

    [Fact]
    public void TestValueCompareIntNotEqualFalse()
    {
        Assert.False(EvalQueryBoolean("12 ne 12"));
    }
    
    [Fact]
    public void TestGeneralCompareIntEqual()
    {
        Assert.True(EvalQueryBoolean("12 = 12"));
    }

    [Fact]
    public void TestGeneralCompareIntEqualFalse()
    {
        Assert.False(EvalQueryBoolean("12 = 13"));
    }
    
    [Fact]
    public void TestGeneralCompareIntNotEqual()
    {
        Assert.True(EvalQueryBoolean("12 != 13"));
    }

    [Fact]
    public void TestGeneralCompareIntNotEqualFalse()
    {
        Assert.False(EvalQueryBoolean("12 != 12"));
    }
}