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
    public void TestValueCompareStringEqualTrue()
    {
        Assert.True(EvalQueryBoolean(@"""test"" eq ""test"""));
    }
    

    [Fact]
    public void TestValueCompareStringEqualFalse()
    {
        Assert.False(EvalQueryBoolean(@"""test"" eq ""test!"""));
    }
    

    [Fact]
    public void TestValueCompareStringNotEqualTrue()
    {
        Assert.True(EvalQueryBoolean(@"""test"" ne ""test!"""));
    }
    

    [Fact]
    public void TestValueCompareStringNotEqualFalse()
    {
        Assert.False(EvalQueryBoolean(@"""test"" ne ""test"""));
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

    [Fact]
    public void TestGeneralCompareStringEqualTrue()
    {
        Assert.True(EvalQueryBoolean(@"""test"" = ""test"""));
    }
    

    [Fact]
    public void TestGeneralCompareStringEqualFalse()
    {
        Assert.False(EvalQueryBoolean(@"""test"" = ""test!"""));
    }
    

    [Fact]
    public void TestGeneralCompareStringNotEqualTrue()
    {
        Assert.True(EvalQueryBoolean(@"""test"" != ""test!"""));
    }
    

    [Fact]
    public void TestGeneralCompareStringNotEqualFalse()
    {
        Assert.False(EvalQueryBoolean(@"""test"" != ""test"""));
    }

}