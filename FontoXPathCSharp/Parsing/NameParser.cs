using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public class NameParser
{
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

    public static ParseFunc<ParseResult<string>> NcName()
    {
        return Then(
            NcNameStartChar(),
            Star(NcNameChar()),
            (a, b) => a + string.Join("", b)
        );
    }
}