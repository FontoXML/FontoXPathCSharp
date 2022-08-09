using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestCompare
{
    private static readonly XmlDocument XmlNodeDocument;
    private static readonly XmlNodeDomFacade XmlNodeDomFacade;
    
    private static readonly XDocument XObjectDocument;
    private static readonly XObjectDomFacade XObjectDomFacade;

    static TestCompare()
    {
        XmlNodeDocument = new XmlDocument();
        XmlNodeDomFacade = new XmlNodeDomFacade();

        XObjectDocument = new XDocument();
        XObjectDomFacade = new XObjectDomFacade();
    }

    private static bool XmlNodeEvalQueryBoolean(string query)
    {
        return Evaluate.EvaluateXPathToBoolean(
            query,
            XmlNodeDocument,
            XmlNodeDomFacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XmlNode>(namespaceResolver: _ => null)
        );
    }
    private static bool XObjectEvalQueryBoolean(string query)
    {
        return Evaluate.EvaluateXPathToBoolean(
            query,
            XObjectDocument,
            XObjectDomFacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XObject>(namespaceResolver: _ => null)
        );
    }
    

    [Fact]
    public void TestValueCompareIntEqual()
    {
        Assert.True(XmlNodeEvalQueryBoolean("12 eq 12"));
        Assert.True(XObjectEvalQueryBoolean("12 eq 12"));
    }

    [Fact]
    public void TestValueCompareIntEqualFalse()
    {
        Assert.False(XmlNodeEvalQueryBoolean("12 eq 13"));
        Assert.False(XObjectEvalQueryBoolean("12 eq 13"));
    }

    [Fact]
    public void TestValueCompareIntNotEqual()
    {
        Assert.True(XmlNodeEvalQueryBoolean("12 ne 13"));
        Assert.True(XObjectEvalQueryBoolean("12 ne 13"));
    }

    [Fact]
    public void TestValueCompareIntNotEqualFalse()
    {
        Assert.False(XmlNodeEvalQueryBoolean("12 ne 12"));
        Assert.False(XObjectEvalQueryBoolean("12 ne 12"));
    }

    [Fact]
    public void TestValueCompareStringEqualTrue()
    {
        Assert.True(XmlNodeEvalQueryBoolean(@"""test"" eq ""test"""));
        Assert.True(XObjectEvalQueryBoolean(@"""test"" eq ""test"""));
    }


    [Fact]
    public void TestValueCompareStringEqualFalse()
    {
        Assert.False(XmlNodeEvalQueryBoolean(@"""test"" eq ""test!"""));
        Assert.False(XObjectEvalQueryBoolean(@"""test"" eq ""test!"""));
    }


    [Fact]
    public void TestValueCompareStringNotEqualTrue()
    {
        Assert.True(XmlNodeEvalQueryBoolean(@"""test"" ne ""test!"""));
        Assert.True(XObjectEvalQueryBoolean(@"""test"" ne ""test!"""));
    }


    [Fact]
    public void TestValueCompareStringNotEqualFalse()
    {
        Assert.False(XmlNodeEvalQueryBoolean(@"""test"" ne ""test"""));
        Assert.False(XObjectEvalQueryBoolean(@"""test"" ne ""test"""));
    }

    [Fact]
    public void TestGeneralCompareIntEqual()
    {
        Assert.True(XmlNodeEvalQueryBoolean("12 = 12"));
        Assert.True(XObjectEvalQueryBoolean("12 = 12"));
    }

    [Fact]
    public void TestGeneralCompareIntEqualFalse()
    {
        Assert.False(XmlNodeEvalQueryBoolean("12 = 13"));
        Assert.False(XObjectEvalQueryBoolean("12 = 13"));
    }

    [Fact]
    public void TestGeneralCompareIntNotEqual()
    {
        Assert.True(XmlNodeEvalQueryBoolean("12 != 13"));
        Assert.True(XmlNodeEvalQueryBoolean("12 != 13"));
    }

    [Fact]
    public void TestGeneralCompareIntNotEqualFalse()
    {
        Assert.False(XmlNodeEvalQueryBoolean("12 != 12"));
        Assert.False(XObjectEvalQueryBoolean("12 != 12"));
    }

    [Fact]
    public void TestGeneralCompareStringEqualTrue()
    {
        Assert.True(XmlNodeEvalQueryBoolean(@"""test"" = ""test"""));
        Assert.True(XObjectEvalQueryBoolean(@"""test"" = ""test"""));
    }


    [Fact]
    public void TestGeneralCompareStringEqualFalse()
    {
        Assert.False(XmlNodeEvalQueryBoolean(@"""test"" = ""test!"""));
        Assert.False(XObjectEvalQueryBoolean(@"""test"" = ""test!"""));
    }


    [Fact]
    public void TestGeneralCompareStringNotEqualTrue()
    {
        Assert.True(XmlNodeEvalQueryBoolean(@"""test"" != ""test!"""));
        Assert.True(XObjectEvalQueryBoolean(@"""test"" != ""test!"""));
    }


    [Fact]
    public void TestGeneralCompareStringNotEqualFalse()
    {
        Assert.False(XmlNodeEvalQueryBoolean(@"""test"" != ""test"""));
        Assert.False(XObjectEvalQueryBoolean(@"""test"" != ""test"""));
    }
}