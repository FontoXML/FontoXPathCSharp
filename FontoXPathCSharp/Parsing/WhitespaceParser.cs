using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public static class WhitespaceParser
{
    public static readonly ParseFunc<ParseResult<string>> WhitespaceCharacter =
        Or(Token(" "));

    private static readonly ParseFunc<ParseResult<string>> ExplicitWhitespace =
        Map(Plus(Token(" ")), x => string.Join("", x));

    public static readonly ParseFunc<ParseResult<string>> Whitespace =
        Map(Star(WhitespaceCharacter), x => string.Join("", x));

    public static readonly ParseFunc<ParseResult<string>> WhitespacePlus =
        Map(Plus(WhitespaceCharacter), x => string.Join("", x));
}