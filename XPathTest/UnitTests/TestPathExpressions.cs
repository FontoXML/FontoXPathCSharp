using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestPathExpressions
{
    private const string TestXml = @"<xml>
        <herp>Herp</herp>
        <derp id=""durp"">derp</derp>
        <derp id=""dorp"">derp</derp>
        <hurr durr=""durrdurrdurr"">durrrrrr</hurr>
    </xml>";

    private const string CommentElementXml = @"<xml>
        <!--comment-->
        <element/>
    </xml>";

    private const string ElementXml = @"<xml>
        <element/>
    </xml>";

    private static readonly XmlDocument XmlNodeDocument;
    private static readonly XmlNodeDomFacade XmlNodeDomfacade;
    private static readonly Options<XmlNode> XmlNodeOptions;

    private static readonly XDocument XObjectDocument;
    private static readonly XObjectDomFacade XObjectDomfacade;
    private static readonly Options<XObject> XObjectOptions;

    private static readonly XmlDocument XmlNodeCommentNodeDocument;
    private static readonly XDocument XObjectCommentNodeDocument;

    private static readonly XmlDocument XmlNodeElementDocument;
    private static readonly XDocument XObjectElementDocument;

    static TestPathExpressions()
    {
        XmlNodeDomfacade = new XmlNodeDomFacade();
        XmlNodeOptions = new Options<XmlNode>(_ => null);
        XObjectDomfacade = new XObjectDomFacade();
        XObjectOptions = new Options<XObject>(_ => null);

        XmlNodeDocument = new XmlDocument();
        XmlNodeDocument.LoadXml(TestXml);
        XObjectDocument = XDocument.Parse(TestXml);

        XmlNodeCommentNodeDocument = new XmlDocument();
        XmlNodeCommentNodeDocument.LoadXml(CommentElementXml);
        XObjectCommentNodeDocument = XDocument.Parse(CommentElementXml);

        XmlNodeElementDocument = new XmlDocument();
        XmlNodeElementDocument.LoadXml(ElementXml);
        XObjectElementDocument = XDocument.Parse(ElementXml);
    }

    private static IEnumerable<XmlNode> XmlNodeEvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            XmlNodeDocument,
            XmlNodeDomfacade,
            XmlNodeOptions
        );
    }

    private static string? XmlNodeEvalQueryString(string query)
    {
        return Evaluate.EvaluateXPathToString(
            query,
            XmlNodeDocument,
            XmlNodeDomfacade,
            XmlNodeOptions
        );
    }

    private static IEnumerable<XObject> XObjectEvalQueryNodes(string query)
    {
        return Evaluate.EvaluateXPathToNodes(
            query,
            XObjectDocument,
            XObjectDomfacade,
            XObjectOptions
        );
    }

    private static string? XObjectEvalQueryString(string query)
    {
        return Evaluate.EvaluateXPathToString(
            query,
            XObjectDocument,
            XObjectDomfacade,
            XObjectOptions
        );
    }

    [Fact]
    public void SimpleAbsolutePath()
    {
        Assert.Single(XmlNodeEvalQueryNodes("/xml/herp"));
        Assert.Single(XObjectEvalQueryNodes("/xml/herp"));
    }

    [Fact]
    public void SimpleRelativePath()
    {
        Assert.Single(XmlNodeEvalQueryNodes("xml/herp"));
        Assert.Single(XObjectEvalQueryNodes("xml/herp"));
    }

    [Fact]
    public void RelativePathEmpty()
    {
        Assert.Empty(XmlNodeEvalQueryNodes("xml/horp"));
        Assert.Empty(XObjectEvalQueryNodes("xml/horp"));
    }

    [Fact]
    public void AbsolutePathEmpty()
    {
        Assert.Empty(XmlNodeEvalQueryNodes("/xml/horp"));
        Assert.Empty(XObjectEvalQueryNodes("/xml/horp"));
    }

    [Fact]
    public void SimpleAttribute()
    {
        Assert.Equal("durrdurrdurr", XmlNodeEvalQueryString("/xml/hurr/@durr"));
        Assert.Equal("durrdurrdurr", XObjectEvalQueryString("/xml/hurr/@durr"));
    }

    [Fact]
    public void AttributeSelect()
    {
        Assert.Single(XmlNodeEvalQueryNodes(@"/xml/derp[@id = ""dorp""]"));
        Assert.Single(XObjectEvalQueryNodes(@"/xml/derp[@id = ""dorp""]"));
    }

    [Fact]
    public void EvalToStringTextContents()
    {
        Assert.Equal("durrrrrr", XmlNodeEvalQueryString("/xml/hurr"));
        Assert.Equal("durrrrrr", XObjectEvalQueryString("/xml/hurr"));
    }

    [Fact]
    public void PrecedingFollowingSiblingsTest()
    {
        const string query = "//element/preceding::node()";

        var commentNode1 = Evaluate.EvaluateXPathToFirstNode(
            query,
            XmlNodeCommentNodeDocument,
            XmlNodeDomfacade,
            XmlNodeOptions
        );

        Assert.True(commentNode1 != null && XmlNodeDomfacade.IsComment(commentNode1));

        var commentNode2 = Evaluate.EvaluateXPathToFirstNode(
            query,
            XObjectCommentNodeDocument,
            XObjectDomfacade,
            XObjectOptions
        );

        Assert.True(commentNode2 != null && XObjectDomfacade.IsComment(commentNode2));
    }

    [Fact]
    public void FollowingSiblingTest()
    {
        const string query = "//comment()/following-sibling::node()";

        var elementNode1 = Evaluate.EvaluateXPathToFirstNode(
            query,
            XmlNodeCommentNodeDocument,
            XmlNodeDomfacade,
            XmlNodeOptions
        );

        Assert.True(elementNode1 != null && XmlNodeDomfacade.IsElement(elementNode1));

        var elementNode2 = Evaluate.EvaluateXPathToFirstNode(
            query,
            XObjectCommentNodeDocument,
            XObjectDomfacade,
            XObjectOptions
        );

        Assert.True(elementNode2 != null && XObjectDomfacade.IsElement(elementNode2));
    }

    [Fact]
    public void AncestorOrSelfTest()
    {
        const string query = "//element/ancestor-or-self::node()";

        var elementAndDocNodes1 = Evaluate.EvaluateXPathToNodes(
            query,
            XmlNodeElementDocument,
            XmlNodeDomfacade,
            XmlNodeOptions
        ).ToList();

        Assert.True(elementAndDocNodes1.Any()
                    && elementAndDocNodes1.Any(XmlNodeDomfacade.IsElement)
                    && elementAndDocNodes1.Any(XmlNodeDomfacade.IsDocument));

        var elementAndDocNodes2 = Evaluate.EvaluateXPathToNodes(
            query,
            XObjectElementDocument,
            XObjectDomfacade,
            XObjectOptions
        ).ToList();

        Assert.True(elementAndDocNodes2.Any()
                    && elementAndDocNodes2.Any(XObjectDomfacade.IsElement)
                    && elementAndDocNodes2.Any(XObjectDomfacade.IsDocument));
    }
}