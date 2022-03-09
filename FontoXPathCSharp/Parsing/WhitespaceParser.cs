using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public static class WhitespaceParser
{
    private static ParseFunc<ParseResult<string>> WhitespaceCharacter()
    {
        return Or(new[]
        {
            Token(" ")
            // TODO: add support for comments
        });
    }

    private static ParseFunc<ParseResult<string>> ExplicitWhitespace()
    {
        return Map(
            Plus(Token(" ")), x => string.Join("", x));
    }

    public static ParseFunc<ParseResult<string>> Whitespace()
    {
        return Map(Star(WhitespaceCharacter()), x => string.Join("", x));
    }

    public static ParseFunc<ParseResult<string>> WhitespacePlus()
    {
        return Map(Plus(WhitespaceCharacter()), x => string.Join("", x));
    }
}