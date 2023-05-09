using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestMisc
{
    private static string AtomicXml = @"<?xml version='1.0' encoding = 'UTF-8'?>
<atomic:root xmlns:atomic=""http://www.w3.org/XQueryTest"">
  <atomic:duration>P1Y2M3DT10H30M</atomic:duration>
</atomic:root>";
    
    
    private static readonly XmlDocument XmlNodeEmptyContext;

    // private static readonly XmlDocument XmlSimpleDocument;
    private static readonly XmlNodeDomFacade XmlNodeDomFacade;
    private static readonly Options<XmlNode> XmlNodeOptions;

    private static readonly XmlDocument XmlNodeWorksMod;
    private static readonly XmlDocument XmlAtomicsFile;
    private static readonly XmlDocument XmlAtomicsSimple;
    
    static TestMisc()
    {
        XmlNodeEmptyContext = new XmlDocument();
        // XmlSimpleDocument = new XmlDocument();
        // XmlSimpleDocument.LoadXml("<p />");
        XmlNodeDomFacade = new XmlNodeDomFacade();
        XmlNodeOptions = new Options<XmlNode>(prefix => prefix == "atomic" ? "http://www.w3.org/XQueryTest" : null);

        XmlNodeWorksMod = new XmlDocument();
        XmlNodeWorksMod.LoadXml(TestFileSystem.ReadFile("qt3tests/docs/works-mod.xml"));
        
        XmlAtomicsFile = new XmlDocument();
        XmlAtomicsFile.LoadXml(TestFileSystem.ReadFile("qt3tests/docs/atomic.xml"));

        XmlAtomicsSimple = new XmlDocument();
        XmlAtomicsSimple.LoadXml(AtomicXml);
    }

    [Fact]
    public void TestFloat()
    {
        Assert.Equal(1,
            Evaluate.EvaluateXPathToInt<string, XmlNode>(
                "xs:float('1')",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    public void TestInstanceOf()
    {
        Assert.False(Evaluate.EvaluateXPathToBoolean<string, XmlNode>(
            "xs:boolean(\"true\") instance of xs:string",
            XmlNodeEmptyContext,
            XmlNodeDomFacade,
            XmlNodeOptions)
        );
    }

    [Fact]
    public void TestPathOrder()
    {
        var document = new XmlDocument();
        document.LoadXml("<x><a/><b/><c/></x>");
        var res = Evaluate
            .EvaluateXPathToNodes("(b,a,c,a)/self::*", document.DocumentElement!, XmlNodeDomFacade, XmlNodeOptions)
            .Select(node => node.Name)
            .ToArray();
        var expected = new[] { "a", "b", "c" };
        Assert.Equal(expected, res);
    }

    [Fact]
    public void TestExpressionCache()
    {
        var selector = string.Concat(Enumerable.Repeat("false() or ", 1000)) + "true()";

        var sw = new Stopwatch();
        sw.Start();
        Evaluate.EvaluateXPathToNodes(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);
        var uncached = sw.Elapsed;
        sw.Restart();
        Evaluate.EvaluateXPathToNodes(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);
        var cached = sw.Elapsed;
        sw.Stop();

        Assert.True(cached < uncached);
    }

    [Fact]
    public void NestedExpression()
    {
        var selector =
            "((((((((((((false() eq false()) eq false()) eq false()) eq " +
            "false()) eq false()) eq false()) eq false()) eq false()) eq " +
            "false()) eq false()) eq false()) eq false()) eq false()";

        var result = Evaluate.EvaluateXPathToBoolean(
            selector,
            XmlNodeEmptyContext,
            XmlNodeDomFacade,
            XmlNodeOptions
        );

        Assert.True(result);
    }


    [Fact]
    public void TextExternalVar()
    {
        var selector = "$x + $y";
        var res = Evaluate.EvaluateXPathToInt(
            selector,
            XmlNodeEmptyContext,
            XmlNodeDomFacade,
            XmlNodeOptions,
            new Dictionary<string, object> { { "x", 1 }, { "y", 2 } }
        );

        Assert.True(res == 3, "Expression should evaluate to 3 (XmlNode)");
    }

    [Fact]
    public void TestDate()
    {
        var selector = "xs:date('2010-11-15Z') cast as xs:string";

        var res = Evaluate.EvaluateXPathToString(
            selector,
            XmlNodeEmptyContext,
            XmlNodeDomFacade,
            XmlNodeOptions
        );

        Assert.Equal("2010-11-15Z", res);
    }

    [Fact]
    public void TestGMonth()
    {
        var selector = "xs:string('--05Z') cast as xs:gMonth";

        var res = Evaluate.EvaluateXPathToString(
            selector,
            XmlNodeEmptyContext,
            XmlNodeDomFacade,
            XmlNodeOptions
        );

        Assert.Equal("--05Z", res);
    }

    [Fact]
    public void AbbreviatedSyntax14()
    {
        var selector = "for $h in (/works/employee[12]/overtime) return $h/../@name";

        var res = Evaluate.EvaluateXPathToString(selector, XmlNodeWorksMod, XmlNodeDomFacade, XmlNodeOptions);

        Assert.Equal("John Doe 12", res);
    }

    [Fact]
    public void InlineFunc()
    {
        var selector = "let $fn := function () as xs:boolean { true() } return $fn()";

        var res = Evaluate.EvaluateXPathToBoolean(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);

        Assert.True(res);
    }

    [Fact]
    public void InlineFunc2()
    {
        var selector = "let $double_funct := function ($arg) { $arg * 2 } return $double_funct(45)";

        var res = Evaluate.EvaluateXPathToInt(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);

        Assert.True(res == 90);
    }

    [Fact]
    public void TestWeirdIntersect()
    {
        var selector = "(/atomic:root/atomic:duration/text()) intersect (/atomic:root/atomic:duration/text())";
        // var selector = "/atomic:root/atomic:duration/text()";
        var res = Evaluate.EvaluateXPathToString(selector, XmlAtomicsFile, XmlNodeDomFacade, XmlNodeOptions);

        Assert.Equal("P1Y2M3DT10H30M",res);
    }

    [Fact]
    public void TestPartialFunctionApplication()
    {
        var selector = "filter(('apple', 'pear', 'apricot', 'avocado', 'orange'),starts-with(?, 'a'))";
        
        var res = Evaluate.EvaluateXPathToStrings(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions).ToArray();
         

        Assert.Equal(new[] { "apple", "apricot", "avocado" }, res);
    }
    
    [Fact]
    public void TestTextTest()
    {
        var selector = "/atomic:root/atomic:duration/text()";
        var res = Evaluate.EvaluateXPathToString(selector, XmlAtomicsSimple, XmlNodeDomFacade, XmlNodeOptions);
        Assert.Equal("P1Y2M3DT10H30M",res);
    }

    [Fact]
    public void TestPredicate()
    {
        var selector = "(1 to 10)[. div 2 = 0]";
        var res = Evaluate.EvaluateXPathToString(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);
        Assert.Equal("", res);
    }
    
    [Fact]
    public void TestPredicate2()
    {
        var selector = "10[. = 1]";
        var res = Evaluate.EvaluateXPathToString(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);
        Assert.Equal("", res);
    }

    [Fact]
    public void TestPredicate3()
    {
        var selector = "bla[. = 1]";
        var res = Evaluate.EvaluateXPathToString(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);
        Assert.Equal("", res);
    }
    
    [Fact]
    public void TestPredicate4()
    {
        var selector = "10[. div 2 = 0]";
        var res = Evaluate.EvaluateXPathToString(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);
        Assert.Equal("", res);
    }
    
    [Fact]
    public void TestPredicate5()
    {
        var selector = "1[2]";
        var res = Evaluate.EvaluateXPathToString(selector, XmlNodeEmptyContext, XmlNodeDomFacade, XmlNodeOptions);
        Assert.Equal("", res);
    }
}