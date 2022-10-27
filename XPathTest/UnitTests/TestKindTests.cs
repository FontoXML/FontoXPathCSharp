using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest.UnitTests;

public class TestKindTests
{
    private readonly XmlDocument _xmlNodeDocument = new();
    private readonly XmlNodeDomFacade _xmlNodeDomFacade = new();
    private readonly Options<XmlNode> _xmlNodeOptions = new(_ => null);
    private readonly XDocument _xObjectDocument = new();

    private readonly XObjectDomFacade _xObjectDomFacade = new();
    private readonly Options<XObject> _xObjectOptions = new(_ => null);

    [Fact]
    private void TestSelfProcessingInstructionTestTrue()
    {
        const string selector = "self::processing-instruction()";

        var pi1 = _xmlNodeDocument.CreateProcessingInstruction("target", "blabla");
        var res1 = Evaluate.EvaluateXPathToBoolean(selector, pi1, _xmlNodeDomFacade, _xmlNodeOptions);
        Assert.True(res1, $"(XmlNode) '{selector}' should return true on a processing instruction.");

        var pi2 = new XProcessingInstruction("target", "blabla");
        var res2 = Evaluate.EvaluateXPathToBoolean(selector, pi2, _xObjectDomFacade, _xObjectOptions);
        Assert.True(res2, $"(XObject) '{selector}' should return true on a processing instruction.");
    }

    [Fact]
    private void TestSelfProcessingInstructionTestFalse()
    {
        const string selector = "self::processing-instruction()";

        var res1 = Evaluate.EvaluateXPathToBoolean(selector, _xmlNodeDocument, _xmlNodeDomFacade, _xmlNodeOptions);
        Assert.False(res1, $"(XmlNode) '{selector}' should return false on a document node.");

        var res2 = Evaluate.EvaluateXPathToBoolean(selector, _xObjectDocument, _xObjectDomFacade, _xObjectOptions);
        Assert.False(res2, $"(XObject) '{selector}' should return false on a document node.");
    }

    [Fact]
    private void TestSelfCommentTestTrue()
    {
        const string selector = "self::comment()";

        var comment = _xmlNodeDocument.CreateComment("comment");
        var res1 = Evaluate.EvaluateXPathToBoolean(selector, comment, _xmlNodeDomFacade, _xmlNodeOptions);
        Assert.True(res1, $"(XmlNode) '{selector}' should return true on an comment.");

        var comment2 = new XComment("comment");
        var res2 = Evaluate.EvaluateXPathToBoolean(selector, comment2, _xObjectDomFacade, _xObjectOptions);
        Assert.True(res2, $"(XObject) '{selector}' should return true on an comment.");
    }

    [Fact]
    private void TestSelfCommentTestFalse()
    {
        const string selector = "self::comment()";

        var res1 = Evaluate.EvaluateXPathToBoolean(selector, _xmlNodeDocument, _xmlNodeDomFacade, _xmlNodeOptions);
        Assert.False(res1, $"(XmlNode) '{selector}' should return false on a document node.");

        var res2 = Evaluate.EvaluateXPathToBoolean(selector, _xObjectDocument, _xObjectDomFacade, _xObjectOptions);
        Assert.False(res2, $"(XObject) '{selector}' should return false on a document node.");
    }
}