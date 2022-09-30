using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public static class WhitespaceParser
{
    // public static Dictionary<int, ParseResult<string>> WhitespaceCache = new();
    // public static Dictionary<int, ParseResult<string>> WhitespacePlusCache = new();

    private static readonly ParseFunc<string> Char = Or(
        Regex("[\t\n\r -\uD7FF\uE000\uFFFD]"),
        Regex("[\uD800-\uDBFF][\uDC00-\uDFFF]")
    );

    private static readonly ParseFunc<string> CommentContents = Preceded(Peek(
        Not(Or(Token("(:"), Token(":)")),
            new[] { "Comment contents cannot contain \"(:\" or \":)\"" })
    ), Char);

    private static readonly ParseFunc<string> Comment = Map(
        Delimited(Token("(:"),
            Star(Or(CommentContents, CommentIndirect)),
            Token(":)")),
        x => string.Join("", x)
    );

    public static readonly ParseFunc<string> WhitespaceCharacter =
        Or(Token("\u0020"), Token("\u0009"), Token("\u000D"), Token("\u000A"), Comment);

    public static readonly ParseFunc<string> Whitespace = 
        Map(Star(WhitespaceCharacter), x => string.Join("", x));
    
    public static readonly ParseFunc<string> WhitespacePlus = 
        Map(Plus(WhitespaceCharacter), x => string.Join("", x));
    
    // public static readonly ParseFunc<string> Whitespace = ParsingFunctions.Cached(
    //     Map(Star(WhitespaceCharacter), x => string.Join("", x)),
    //     WhitespaceCache
    // );
    //
    // public static readonly ParseFunc<string> WhitespacePlus = ParsingFunctions.Cached(
    //     Map(Plus(WhitespaceCharacter), x => string.Join("", x)),
    //     WhitespacePlusCache
    // );

    private static ParseResult<string> CommentIndirect(string input, int offset)
    {
        return Comment(input, offset);
    }
}