namespace prscsharp;

using static PrscSharp;

static class XPathParser
{

    public static ParseFunc<ParseResult<string>> ForwardAxis()
    {
        return Map(Or(new[]
        {
            Token("self::")
            // TODO: add other variants
        }), (x) => x[..^2]);
    }

    public static ParseFunc<ParseResult<string>> NcNameStartChar()
    {
        return Or(new[]
        {
            Regex(
                @"[A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD]"),
            Then(Regex(@"[\uD800-\uDB7F]"), Regex(@"[\uDC00-\uDFFF]"), (a, b) => a + b),
        });
    }

    public static ParseFunc<ParseResult<string>> NcNameChar()
    {
        return Or(new[]
        {
            NcNameStartChar(),
            Regex(@"[\-\.0-9\xB7\u0300-\u036F\u203F\u2040]"),
        });
    }

    public static ParseFunc<ParseResult<string>> NcName()
    {
        return Then(
            NcNameStartChar(),
            Star(NcNameChar()),
            (a, b) => a + string.Join("", b)
        );
    }

    public static ParseFunc<ParseResult<object[]>> UnprefixedName()
    {
        return Map(NcName(), x =>
            new[]
            {
                x as object
            });
    }

    public static ParseFunc<ParseResult<object[]>> QName()
    {
        return Or(new[]
        {
            UnprefixedName(),
            // TODO: add prefixed name
        });
    }

    public static ParseFunc<ParseResult<object[]>> EqName()
    {
        return Or(new[]
        {
            QName(),
            // TODO: add other options
        });
    }

    public static ParseFunc<ParseResult<object[]>> NameTest()
    {
        return Map(EqName(), x => new object[] { "nameTest", x });
    }

    public static ParseFunc<ParseResult<object[]>> NodeTest()
    {
        return Or(new[] { NameTest() });
    }

    public static ParseFunc<ParseResult<object[]>> ForwardStep()
    {
        return Or(new[]
        {
            Then(ForwardAxis(), NodeTest(),
                (axis, test) => new[] {"stepExpr" as object, new[] {"xpathAxis", axis as object}, test}),
        });
    }

    public static ParseFunc<ParseResult<object[]>> AxisStep()
    {
        // TODO: add predicateList
        return Or(new[]
        {
            ForwardStep(),
            // TODO: add reverse step
        });
    }

    public static ParseFunc<ParseResult<object[]>> StepExprWithForcedStep()
    {
        // TODO: add postfix expr with step
        return Or(new[]
        {
            AxisStep(),
        });
    }

    public static ParseFunc<ParseResult<object[]>> RelativePathExpr()
    {
        return Or(new[]
        {
            // TODO: add other variants
            Map(StepExprWithForcedStep(), x => new[] {"pathExpr" as object, x}),
        });
    }

    public static ParseFunc<ParseResult<object[]>> PathExpr()
    {
        return Or(new[]
        {
            RelativePathExpr(),
            // TODO: add other variants
        });
    }
}

internal static class Program
{
    private static void PrintAst(IEnumerable<object> ast, int indent = 0)
    {
        foreach (var a in ast)
        {
            if (a is object[] objects)
                PrintAst(objects, indent + 1);
            else
                Console.WriteLine(new string('\t', indent) + a);
        }
    }

    public static void Main()
    {
        var parser = XPathParser.PathExpr();

        var result = parser("self::p", 0).Unwrap();
        Console.WriteLine("Result:");
        PrintAst(result);
    }
}
