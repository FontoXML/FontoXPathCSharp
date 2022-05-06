using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public static class WhitespaceParser
{
    public static readonly ParseFunc<string> WhitespaceCharacter =
        Or(Token(" "));

    private static readonly ParseFunc<string> ExplicitWhitespace =
        Map(Plus(Token(" ")), x => string.Join("", x));

    public static readonly ParseFunc<string> Whitespace =
        Map(Star(WhitespaceCharacter), x => string.Join("", x));

    public static readonly ParseFunc<string> WhitespacePlus =
        Map(Plus(WhitespaceCharacter), x => string.Join("", x));
}