using FontoXPathCSharp.Expressions;
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

    private static ParseFunc<ParseResult<QName>> UnprefixedName()
    {
        return Map(NcName(), x =>
            new QName(x, null, ""));
    }

    private static ParseFunc<ParseResult<QName>> QName()
    {
        return Or(new[]
        {
            UnprefixedName()
            // TODO: add prefixed name
        });
    }

    private static ParseFunc<ParseResult<QName>> EqName()
    {
        return Or(new[]
        {
            QName()
            // TODO: add other options
        });
    }

    private static ParseFunc<ParseResult<Ast>> NameTest()
    {
        // TODO: add wildcard
        return Map(EqName(), x =>
            new Ast("nameTest")
            {
                StringAttributes =
                {
                    ["URI"] = x.NamespaceUri!,
                    ["prefix"] = x.Prefix!
                },
                TextContent = x.LocalName
            });
    }

    private static ParseFunc<ParseResult<Ast>> NodeTest()
    {
        return Or(new[] {NameTest()});
    }

    private static ParseFunc<ParseResult<Ast>> ForwardStep()
    {
        return Or(new[]
        {
            Then(ForwardAxis(), NodeTest(),
                (axis, test) =>
                {
                    var ast = new Ast("stepExpr");
                    ast.Children.Add(new Ast("xpathAxis") {TextContent = axis});
                    ast.Children.Add(test);
                    return ast;
                })
        });
    }

    private static ParseFunc<ParseResult<Ast>> AxisStep()
    {
        // TODO: add predicateList
        return Or(new[]
        {
            ForwardStep()
            // TODO: add reverse step
        });
    }

    private static ParseFunc<ParseResult<Ast>> StepExprWithForcedStep()
    {
        // TODO: add postfix expr with step
        return Or(new[]
        {
            AxisStep()
        });
    }

    private static ParseFunc<ParseResult<Ast>> RelativePathExpr()
    {
        return Or(new[]
        {
            // TODO: add other variants
            Map(StepExprWithForcedStep(), x =>
            {
                var ast = new Ast("pathExpr");
                ast.Children.Add(x);
                return ast;
            })
        });
    }

    public static ParseFunc<ParseResult<Ast>> PathExpr()
    {
        return Or(new[]
        {
            RelativePathExpr()
            // TODO: add other variants
        });
    }
}