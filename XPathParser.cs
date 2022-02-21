namespace prscsharp;

static class XPathParser
{
    public static ParseFunc<ParseResult<string>> ForwardAxis()
    {
        return PrscSharp.Map(PrscSharp.Or(new[]
        {
            PrscSharp.Token("self::")
            // TODO: add other variants
        }), (x) => x[..^2]);
    }

    public static ParseFunc<ParseResult<string>> NcNameStartChar()
    {
        return PrscSharp.Or(new[]
        {
            PrscSharp.Regex(
                @"[A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD]"),
            PrscSharp.Then(PrscSharp.Regex(@"[\uD800-\uDB7F]"), PrscSharp.Regex(@"[\uDC00-\uDFFF]"), (a, b) => a + b),
        });
    }
    
    public static ParseFunc<ParseResult<string>> NcNameChar()
    {
        return PrscSharp.Or(new[]
        {
            NcNameStartChar(),
            PrscSharp.Regex(@"[\-\.0-9\xB7\u0300-\u036F\u203F\u2040]"),
        });
    }
    
    public static ParseFunc<ParseResult<string>> NcName()
    {
        return PrscSharp.Then(
            NcNameStartChar(),
            PrscSharp.Star(NcNameChar()),
            (a, b) => a + string.Join("", b)
        );
    }

    public static ParseFunc<ParseResult<object[]>> UnprefixedName()
    {
        return PrscSharp.Map(NcName(), x =>
            new[]
            {
                x as object
            });
    }

    public static ParseFunc<ParseResult<object[]>> QName()
    {
        return PrscSharp.Or(new[]
        {
            UnprefixedName(),
            // TODO: add prefixed name
        });
    }

    public static ParseFunc<ParseResult<object[]>> EqName()
    {
        return PrscSharp.Or(new[]
        {
            QName(),
            // TODO: add other options
        });
    }

    public static ParseFunc<ParseResult<object[]>> NameTest()
    {
        return PrscSharp.Map(EqName(), x => new object[] {"nameTest", x});
    }

    public static ParseFunc<ParseResult<object[]>> NodeTest()
    {
        return PrscSharp.Or(new[] {NameTest()});
    }

    public static ParseFunc<ParseResult<object[]>> ForwardStep()
    {
        return PrscSharp.Or(new[]
        {
            PrscSharp.Then(ForwardAxis(), NodeTest(),
                (axis, test) => new[] {"stepExpr" as object, new[] {"xpathAxis", axis as object}, test}),
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
        var parser = XPathParser.ForwardStep();

        var result = parser("self::p", 0).Unwrap();
        Console.WriteLine("Result:");
        PrintAst(result);
    }
}