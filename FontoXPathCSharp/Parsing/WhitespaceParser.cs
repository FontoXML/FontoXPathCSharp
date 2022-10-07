using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public class WhitespaceParser
{
    private readonly ParseFunc<string> Char;

    private readonly ParseFunc<string> Comment;

    private readonly ParseFunc<string> CommentContents;

    public readonly ParseFunc<string> Whitespace;
    private readonly Dictionary<int, ParseResult<string>> WhitespaceCache;

    public readonly ParseFunc<string> WhitespaceCharacter;

    public readonly ParseFunc<string> WhitespacePlus;
    private readonly Dictionary<int, ParseResult<string>> WhitespacePlusCache;

    public WhitespaceParser()
    {
        WhitespaceCache = new Dictionary<int, ParseResult<string>>();
        WhitespacePlusCache = new Dictionary<int, ParseResult<string>>();

        Char = Or(
            Regex("[\t\n\r -\uD7FF\uE000\uFFFD]"),
            Regex("[\uD800-\uDBFF][\uDC00-\uDFFF]")
        );

        CommentContents = Preceded(Peek(
            Not(Or(Token("(:"), Token(":)")),
                new[] { "Comment contents cannot contain \"(:\" or \":)\"" })
        ), Char);

        Comment = Map(
            Delimited(Token("(:"),
                Star(Or(CommentContents, CommentIndirect)),
                Token(":)")),
            x => string.Join("", x)
        );

        WhitespaceCharacter = Or(Token("\u0020"), Token("\u0009"), Token("\u000D"), Token("\u000A"), Comment);

        Whitespace = ParsingFunctions.Cached(
            Map(Star(WhitespaceCharacter), x => string.Join("", x)),
            WhitespaceCache
        );

        WhitespacePlus = ParsingFunctions.Cached(
            Map(Plus(WhitespaceCharacter), x => string.Join("", x)),
            WhitespacePlusCache
        );
    }

    private ParseResult<string> CommentIndirect(string input, int offset)
    {
        return Comment(input, offset);
    }
}