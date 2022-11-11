using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestAxis
{
    private const string TestXml = @"<xml>
  <title>xpath.playground.fontoxml.com</title>
  <summary>This is a learning tool for XML, XPath and XQuery.</summary>
  <tips>
    <tap></tap>
    <tip id='edit'>You can edit everything on the left</tip>
    <tip id='examples'>You can access more examples from a menu in the top right</tip>
    <tup></tup>
    <tip id='permalink'>Another button there lets you share your test using an URL</tip>
  </tips>
</xml>";

    private const string Tree1ChildXml = @"<?xml version='1.0' encoding='UTF-8'?>
    <far-north>
    <north>
    <near-north>
    <far-west/>
    <west mark='w0'/>
        <near-west/>
    <center mark='c0'><the1child/></center>
    <near-east/>
    <east mark='e0'>Text in east</east>
    <far-east/>
    </near-north>
    </north>
    </far-north>
    ";

    private const string TreeCompassXml = @"<?xml version='1.0' encoding='UTF-8'?>
    <far-north> text-1A
        <!-- Comment-2 --> text-1B
        <?a-pi pi-1?> text-1C
        <north mark='n0'> text-2A
        <!-- Comment-3 --> text-2B
        <?a-pi pi-2?> text-2C
        <near-north> text-3A
        <far-west/> text-3B
        <west mark='w0' west-attr-1='w1' west-attr-2='w2' west-attr-3='w3'/> text-3C
        <near-west/> text-3D
    <!-- Comment-4 --> text-3E
    <?a-pi pi-3?> text-3F
    <center mark='c0' center-attr-1='c1' center-attr-2='c2' center-attr-3='c3'> text-4A
        <near-south-west/> text-4B
        <!--Comment-5--> text-4C
        <?a-pi pi-4?> text-4D
    <near-south> text-5A
        <!--Comment-6--> text-5B
        <?a-pi pi-5?> text-5C
        <south mark='s0' south-attr-1='s1' south-attr-2='s2'> text-6A
        <far-south/> text-6B
        </south> text-5D
    </near-south> text-4E
    <south-east mark='se'/> text-4F
                     </center> text-3G
        <near-east/> text-3H
        <east mark='e0'>Text in east</east> text-3I
        <far-east/> text-3J
        </near-north> text-2D
    </north> text-1D
    </far-north>
    ";

    private static readonly XmlDocument XmlNodeDocument;
    private static readonly XmlNodeDomFacade XmlNodeDomFacade;

    private static readonly XDocument XObjectDocument;
    private static readonly XObjectDomFacade XObjectDomFacade;

    private static readonly XmlDocument Tree1XmlDocument;
    private static readonly XDocument Tree1XObjectDocument;

    private static readonly XmlDocument TreeCompassXmlDocument;
    private static readonly XDocument TreeCompassXObjectDocument;

    static TestAxis()
    {
        XmlNodeDocument = new XmlDocument();
        XmlNodeDocument.LoadXml(TestXml);
        XmlNodeDomFacade = new XmlNodeDomFacade();

        XObjectDocument = XDocument.Parse(TestXml);
        XObjectDomFacade = new XObjectDomFacade();

        Tree1XmlDocument = new XmlDocument();
        Tree1XmlDocument.LoadXml(Tree1ChildXml);
        Tree1XObjectDocument = XDocument.Parse(Tree1ChildXml);

        TreeCompassXmlDocument = new XmlDocument();
        TreeCompassXmlDocument.LoadXml(TreeCompassXml);
        TreeCompassXObjectDocument = XDocument.Parse(TreeCompassXml);
    }

    private static IEnumerable<XmlNode> XmlNodeEvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            XmlNodeDocument,
            XmlNodeDomFacade,
            new Options<XmlNode>(_ => null)
        );
    }

    private static IEnumerable<XObject> XObjectEvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            XObjectDocument,
            XObjectDomFacade,
            new Options<XObject>(_ => null)
        );
    }

    [Fact]
    public void TestAncestorAxis()
    {
        Assert.Single(XmlNodeEvalQueryNodes("/xml/tips/tip/ancestor::xml"));
        Assert.Single(XObjectEvalQueryNodes("/xml/tips/tip/ancestor::xml"));
    }

    [Fact]
    public void TestAncestorAxisSelf()
    {
        Assert.Empty(XmlNodeEvalQueryNodes("/xml/tips/tip/ancestor::tip"));
        Assert.Empty(XObjectEvalQueryNodes("/xml/tips/tip/ancestor::tip"));
    }

    [Fact]
    public void TestDescendantAxis()
    {
        Assert.Equal(3, XmlNodeEvalQueryNodes("descendant::tip").Count());
        Assert.Equal(3, XObjectEvalQueryNodes("descendant::tip").Count());
    }

    [Fact]
    public void TestFollowingAxis()
    {
        Assert.Equal(3, XmlNodeEvalQueryNodes(@"/xml/tips/tap/following::tip").Count());
        Assert.Equal(3, XObjectEvalQueryNodes(@"/xml/tips/tap/following::tip").Count());
    }

    [Fact]
    public void TestPrecedingAxis()
    {
        Assert.Equal(2, XmlNodeEvalQueryNodes(@"/xml/tips/tup/preceding::tip").Count());
        Assert.Equal(2, XObjectEvalQueryNodes(@"/xml/tips/tup/preceding::tip").Count());
    }

    [Fact]
    public void TestFollowingSiblingAxis()
    {
        Assert.Single(XmlNodeEvalQueryNodes(@"/xml/tips/tup/following-sibling::tip"));
        Assert.Single(XObjectEvalQueryNodes(@"/xml/tips/tup/following-sibling::tip"));
    }

    [Fact]
    public void TestPrecedingSiblingAxis()
    {
        Assert.Equal(2, XObjectEvalQueryNodes(@"/xml/tips/tup/preceding-sibling::tip").Count());
        Assert.Equal(2, XmlNodeEvalQueryNodes(@"/xml/tips/tup/preceding-sibling::tip").Count());
    }

    [Fact]
    public void TestDescendantAxis2()
    {
        var selector = "fn:count(//center/child::*)";

        var res = Evaluate.EvaluateXPathToInt(
            selector,
            Tree1XmlDocument,
            XmlNodeDomFacade,
            new Options<XmlNode>(_ => null)
        );

        Assert.Equal(1, res);

        res = Evaluate.EvaluateXPathToInt(
            selector,
            Tree1XObjectDocument,
            XObjectDomFacade,
            new Options<XObject>(_ => null)
        );

        Assert.Equal(1, res);
    }

    [Fact]
    public void TestDescendantAxis3()
    {
        var selector = "fn:count(//center/descendant::node())";

        var res = Evaluate.EvaluateXPathToInt(
            selector,
            TreeCompassXmlDocument,
            XmlNodeDomFacade,
            new Options<XmlNode>(_ => null)
        );

        Assert.Equal(21, res);
    }

    // [Fact]
    // public void TestQt3TestVarRefExternal()
    // {
    //     var selector = "let $x := 'n0' return /far-north/north[$x=fn:last()]";
    //
    //     var res = Evaluate.EvaluateXPathToNodes(
    //         selector,
    //         TreeCompassXmlDocument,
    //         XmlNodeDomFacade,
    //         new Options<XmlNode>(_ => null)
    //     );
    //
    //     Assert.Single(res);
    // }

    // [Fact]
    // public void TestQt3TestVarRefQName()
    // {
    //     var selector = "let $x := 1 return $x";
    //
    //     var res = Evaluate.EvaluateXPathToInt(
    //         selector,
    //         TreeCompassXmlDocument,
    //         XmlNodeDomFacade,
    //         new Options<XmlNode>(_ => null)
    //     );
    //
    //     Assert.Equal(1, res);
    // }
}