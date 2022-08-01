using System.Xml;
using FontoXPathCSharp;
using Xunit;

namespace XPathTest.UnitTests;

public class TestMisc
{
    [Fact]
    public void TestFloat()
    {
        Assert.Equal(1, Evaluate.EvaluateXPathToInt<string, XmlNode>("xs:float('1')", null));
    }

    [Fact]
    public void TestInstanceOf()
    {
        Assert.False(
            Evaluate.EvaluateXPathToBoolean<string, XmlNode>("xs:boolean(\"true\") instance of xs:string", null));
    }
}