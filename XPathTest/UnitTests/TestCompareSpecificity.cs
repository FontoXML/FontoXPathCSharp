using System.Xml;
using FontoXPathCSharp;
using Xunit;

namespace XPathTest.UnitTests;

public class TestCompareSpecificity
{
    private static void AssertSpecificity(
        string selectorExpressionA,
        string selectorExpressionB,
        int expectedResult
    )
    {
        Assert.Equal(
            ExpressionUtils<XmlNode, string>.CompareSpecificity(selectorExpressionA, selectorExpressionB),
            expectedResult
        );
    }

    [Fact]
    public void Return0ForSamePath()
    {
        AssertSpecificity("self::*", "self::*", 0);
    }

    [Fact]
    public void NodeTypeGreaterUniversal()
    {
        AssertSpecificity("self::element()", "self::node()", 1);
    }

    [Fact]
    public void NameGreaterNodeType()
    {
        AssertSpecificity("self::name", "self::element()", 1);
    }

    [Fact]
    public void AttributeGreaterNodeName()
    {
        AssertSpecificity("@id", "self::name", 1);
    }

    [Fact]
    public void FunctionsGreaterAttributes()
    {
        AssertSpecificity("id('123')", "@id", 1);
    }

    [Fact]
    public void UnionIsMaximumOperands()
    {
        AssertSpecificity("self::name | id('123') | self::otherName | id('123')", "self::name", 1);
    }
}