using FontoXPathCSharp.Parsing;
using Xunit;

namespace XPathTest.UnitTests;

public class TestParser
{
    private static void ParseQuery(string query)
    {
        var result = XPathParser.Parse(query, new ParseOptions(false, false));
        Assert.True(result.IsOk(), "Query: " + query + "\n" + result);
    }

    [Fact]
    public void ParseStringLiteralDoubleQuote()
    {
        ParseQuery("\"test'\"");
    }

    [Fact]
    public void ParseStringLiteralSingleQuote()
    {
        ParseQuery("'test\"'");
    }

    [Fact]
    public void ParseEmptyElementTest()
    {
        ParseQuery("element()");
    }

    [Fact]
    public void ParseWildcardElementTest()
    {
        ParseQuery("element(*)");
    }

    [Fact]
    public void ParseWildcardTypenameElementTest()
    {
        ParseQuery("element(*, test-type-name)");
    }

    [Fact]
    public void ParseEmptyAttributeTest()
    {
        ParseQuery("attribute()");
    }

    [Fact]
    public void ParseWildcardAttributeTest()
    {
        ParseQuery("attribute(*)");
    }

    [Fact]
    public void ParseWildcardTypenameAttributeTest()
    {
        ParseQuery("attribute(*, test-type-name)");
    }

    [Fact]
    public void ParseSchemaElementTest()
    {
        ParseQuery("schema-element(test-element-declaration)");
    }

    [Fact]
    public void ParseSchemaAttributeTest()
    {
        ParseQuery("schema-attribute(test-attribute-declaration)");
    }

    [Fact]
    public void ParseEmptyPiTest()
    {
        ParseQuery("processing-instruction()");
    }

    [Fact]
    public void ParsePiTest()
    {
        ParseQuery("processing-instruction(test-name)");
    }

    [Fact]
    public void ParseEmptyDocumentTest()
    {
        ParseQuery("document-node()");
    }

    [Fact]
    public void ParseDocumentTest()
    {
        ParseQuery("document-node(element())");
    }

    [Fact]
    public void ParseWildcardPreceded()
    {
        ParseQuery("*:test");
    }

    [Fact]
    public void ParseWildcard()
    {
        ParseQuery("*");
    }

    [Fact]
    public void ParseWildcardBracedUriLiteral()
    {
        ParseQuery("Q{x}*");
    }

    [Fact]
    public void ParseWildcardFollowed()
    {
        ParseQuery("xml:*");
    }

    [Fact]
    public void ParseForwardStep()
    {
        ParseQuery("self::x");
    }

    [Fact]
    public void ParseAbbrevReverseStep()
    {
        ParseQuery("..");
    }

    [Fact]
    public void ParseReverseStep()
    {
        ParseQuery("parent::x");
    }

    [Fact]
    public void ParsePredicate()
    {
        ParseQuery("self::x[test]");
    }

    [Fact]
    public void ParseNumericLiteral()
    {
        ParseQuery("12e-4");
    }

    [Fact]
    public void ParseFunctionCall()
    {
        ParseQuery("node-name(/xml)");
    }

    [Fact]
    public void ParseParenthesizedExpr()
    {
        ParseQuery("(12)");
    }

    [Fact]
    public void ParseEmptyParenthesizedExpr()
    {
        ParseQuery("( )");
    }

    [Fact]
    public void ParseContextItem()
    {
        ParseQuery(".");
    }

    [Fact]
    public void ParseRelativePathExpr()
    {
        ParseQuery("xml/test");
    }

    [Fact]
    public void ParseRelativePathAbbrevExpr()
    {
        ParseQuery("xml//test");
    }

    [Fact]
    public void ParseAbsolutePathExpr()
    {
        ParseQuery("/xml/test");
    }

    [Fact]
    public void ParseAbsolutePathAbbrevExpr()
    {
        ParseQuery("//test");
    }

    [Fact]
    public void ParseAbsoluteRootExpr()
    {
        ParseQuery("/");
    }

    [Fact]
    public void ParseUnaryMinusExpr()
    {
        ParseQuery("-12");
    }

    [Fact]
    public void ParseUnaryPlusExpr()
    {
        ParseQuery("+12");
    }

    [Fact]
    public void ParseVarRef()
    {
        ParseQuery("$test");
    }

    [Fact]
    public void ParseArrowExpr()
    {
        ParseQuery("x => test(x)");
    }

    [Fact]
    public void ParseCastExpr()
    {
        ParseQuery("x cast as test?");
    }

    // [Fact]
    // public void ParseCastableExpr() =>
    //     ParseQuery("x castable as test?");

    // [Fact]
    // public void ParseTreatExpr() =>
    //     ParseQuery("x treat as test?");

    [Fact]
    public void ParseIntersectExpr()
    {
        ParseQuery("x intersect y");
    }

    [Fact]
    public void ParseExceptExpr()
    {
        ParseQuery("x except y");
    }

    [Fact]
    public void ParseUnionExpr()
    {
        ParseQuery("x union y union z");
    }

    [Fact]
    public void ParseMultiplicativeExpr()
    {
        ParseQuery("x * y div z idiv w mod u");
    }

    [Fact]
    public void ParseAdditiveExpr()
    {
        ParseQuery("12 + 13 - 14");
    }

    [Fact]
    public void ParseRangeExpr()
    {
        ParseQuery("12 to 14");
    }

    [Fact]
    public void ParseStringConcatExpr()
    {
        ParseQuery("\"foo\" || \"bar\"");
    }

    [Fact]
    public void ParseWhitespaceNewLine()
    {
        ParseQuery("1 +\n2");
    }

    [Fact]
    public void ParseComment()
    {
        ParseQuery("1 + (: test :) 2");
    }

    [Fact]
    public void ParseCombinedPathExpr()
    {
        ParseQuery(@"$dependencies/@value/tokenize(.)");
    }

    [Fact]
    public void ParseNestedFunctionCall()
    {
        ParseQuery(@"not(exists($dependencies[@type=""xml-version"" and @value=""1.1""]))");
    }

    [Fact]
    public void ParseComplex()
    {
        ParseQuery(@"
/test-set/test-case[
    not(exists((./dependency | ../dependency)[@type=""xml-version"" and @value=""1.1""])) and not(
     (./dependency | ../dependency)/@value/tokenize(.) = (
       ""XQ10"",
       ""XQ20"",
       ""XQ30"",
       ""schemaValidation"",
       ""schemaImport"",
       (:""staticTyping"",:)
       (:""serialization"",:)
       ""infoset-dtd"",
       (:""xpath-1.0-compatibility"",:)
       ""namespace-axis"",
       (:""moduleImport"",:)
       ""schema-location-hint"",
       (:""collection-stability"",:)
       ""directory-as-collation-uri"",
       (:""fn-transform-XSLT"",:)
       (:""fn-transform-XSLT30"",:)
       (:""fn-format-integer-CLDR"",:)
       (:""non-empty-sequence-collection"",:)
       ""non-unicode-codepoint-collation"",
       ""simple-uca-fallback"",
       ""advanced-uca-fallback""))]
");
    }

    [Fact]
    public void ParsePrefixedFunction()
    {
        ParseQuery("fn:not()");
    }

    [Fact]
    public void ParseBracedUriLiteral()
    {
        ParseQuery("Q{x}abc");
    }
}