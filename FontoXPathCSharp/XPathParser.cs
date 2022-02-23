using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp;

public static class XPathParser
{
    private static ParseFunc<ParseResult<string>> ForwardAxis()
    {
        return Map(Or(new[]
        {
            Token("self::")
            // TODO: add other variants
        }), x => x[..^2]);
    }

    private static ParseFunc<ParseResult<string>> NcNameStartChar()
    {
        return Or(new[]
        {
            Regex(
                @"[A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD]"),
            Then(Regex(@"[\uD800-\uDB7F]"), Regex(@"[\uDC00-\uDFFF]"), (a, b) => a + b)
        });
    }

    private static ParseFunc<ParseResult<string>> NcNameChar()
    {
        return Or(new[]
        {
            NcNameStartChar(),
            Regex(@"[\-\.0-9\xB7\u0300-\u036F\u203F\u2040]")
        });
    }

    private static ParseFunc<ParseResult<string>> NcName()
    {
        return Then(
            NcNameStartChar(),
            Star(NcNameChar()),
            (a, b) => a + string.Join("", b)
        );
    }

    private static ParseFunc<ParseResult<object[]>> UnprefixedName()
    {
        return Map(NcName(), x =>
            new[]
            {
                x as object
            });
    }

    private static ParseFunc<ParseResult<object[]>> QName()
    {
        return Or(new[]
        {
            UnprefixedName()
            // TODO: add prefixed name
        });
    }

    private static ParseFunc<ParseResult<object[]>> EqName()
    {
        return Or(new[]
        {
            QName()
            // TODO: add other options
        });
    }

    private static ParseFunc<ParseResult<object[]>> NameTest()
    {
        return Map(EqName(), x => new object[] {"nameTest", x});
    }

    private static ParseFunc<ParseResult<object[]>> NodeTest()
    {
        return Or(new[] {NameTest()});
    }

    private static ParseFunc<ParseResult<object[]>> ForwardStep()
    {
        return Or(new[]
        {
            Then(ForwardAxis(), NodeTest(),
                (axis, test) => new[] {"stepExpr" as object, new[] {"xpathAxis", axis as object}, test})
        });
    }

    private static ParseFunc<ParseResult<object[]>> AxisStep()
    {
        // TODO: add predicateList
        return Or(new[]
        {
            ForwardStep()
            // TODO: add reverse step
        });
    }

    private static ParseFunc<ParseResult<object[]>> StepExprWithForcedStep()
    {
        // TODO: add postfix expr with step
        return Or(new[]
        {
            AxisStep()
        });
    }

    private static ParseFunc<ParseResult<object[]>> RelativePathExpr()
    {
        return Or(new[]
        {
            // TODO: add other variants
            Map(StepExprWithForcedStep(), x => new[] {"pathExpr" as object, x})
        });
    }

    public static ParseFunc<ParseResult<object[]>> PathExpr()
    {
        return Or(new[]
        {
            RelativePathExpr()
            // TODO: add other variants
        });
    }
}