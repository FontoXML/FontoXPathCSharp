using FontoXPathCSharp;
using Xunit;

namespace XPathTest.UnitTests;

public class TestMisc
{
    [Fact]
    public void TestFloat()
    {
        Assert.Equal(1, Evaluate.EvaluateXPathToInt("xs:float('1')", null));
    }
    
    [Fact]
    public void TestInstanceOf()
    {
        Assert.False(Evaluate.EvaluateXPathToBoolean("xs:boolean(\"true\") instance of xs:string", null));
    }
}