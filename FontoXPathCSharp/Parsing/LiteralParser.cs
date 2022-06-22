using PrscSharp;
using static PrscSharp.PrscSharp;
using static FontoXPathCSharp.Parsing.WhitespaceParser;
using static FontoXPathCSharp.Parsing.ParsingFunctions;

namespace FontoXPathCSharp.Parsing;

public static class LiteralParser
{
    public static readonly ParseFunc<string> AssertAdjacentOpeningTerminal =
        Peek(Or(
            Token("("), Token("\""), Token("'"), WhitespaceCharacter));

    public static readonly ParseFunc<string> ForwardAxis =
        Map(Or(
            Token("child::"),
            Token("descendant::"),
            Token("attribute::"),
            Token("self::"),
            Token("descendant-or-self::"),
            Token("following-sibling::"),
            Token("following::")
        ), x => x[..^2]);

    public static readonly ParseFunc<string> ReverseAxis =
        Map(Or(
            Token("parent::"),
            Token("ancestor::"),
            Token("preceding-sibling::"),
            Token("preceding::"),
            Token("ancestor-or-self::")
        ), x => x[..^2]);

    public static readonly ParseFunc<string> PredefinedEntityRef = Then3(
        Token("&"),
        Or(Token("<"), Token(">"), Token("&"), Token("'"), Token("\"")),
        Token(";"),
        (a, b, c) => a + b + c
    );

    public static readonly ParseFunc<string> CharRef = Or(
        Then3(Token("&#x"), Regex("[0 - 9a - fA - F] + "), Token(";"), (a, b, c) => a + b + c),
        Then3(Token("&#"), Regex("[0-9]+"), Token(";"), (a, b, c) => a + b + c)
    );

    public static readonly ParseFunc<string> EscapeQuot = Alias("\"", "\"\"");
    public static readonly ParseFunc<string> EscapeApos = Alias("'", "''");

    private static readonly ParseFunc<string> Digits =
        Regex(@"[0-9]+");

    private static readonly ParseFunc<Ast> DoubleLiteral = Then(
        Or(
            Then(Token("."), Digits, (dot, digitsParsed) => dot + digitsParsed),
            Then(
                Digits,
                Optional(Preceded(Token("."), Regex("/[0-9]*/"))),
                (a, b) => a + (b != null ? "." + b : "")
            )
        ),
        Then3(
            Or(Token("e"), Token("E")),
            Optional(Or(Token("+"), Token("-"))),
            Digits,
            (e, expSign, expDigits) => e + (expSign ?? "") + expDigits
        ),
        (b, exponent) =>
            new Ast(AstNodeName.DoubleConstantExpr, new Ast(AstNodeName.Value) {TextContent = b + exponent})
    );

    private static readonly ParseFunc<Ast> DecimalLiteral = Or(
        Map(Preceded(Token("."), Digits),
            x => new Ast(AstNodeName.DecimalConstantExpr,
                new Ast(AstNodeName.Value) {TextContent = "." + x}
            )),
        Then(Followed(Digits, Token(".")), Optional(Digits),
            (first, second) => new Ast(AstNodeName.DecimalConstantExpr,
                new Ast(AstNodeName.Value) {TextContent = first + "." + (second ?? "")}))
    );

    private static readonly ParseFunc<Ast> IntegerLiteral =
        Map(Digits, d =>
            new Ast(AstNodeName.IntegerConstantExpr, new Ast(AstNodeName.Value)
            {
                TextContent = d
            }));

    public static readonly ParseFunc<Ast> NumericLiteral =
        Followed(
            Or(DoubleLiteral, DecimalLiteral, IntegerLiteral),
            Peek(Not(Regex(@"[a-z][A-Z]"), new[] {"No alphabetic characters after numeric literal"}))
        );

    public static readonly ParseFunc<Ast> ContextItemExpr =
        Map(Followed(Token("."), Peek(Not(Token("."), new[] {"context item should not be followed by another ."}))),
            _ => new Ast(AstNodeName.ContextItemExpr));

    public static readonly ParseFunc<string> ReservedFunctionNames =
        Or(new[]
        {
            "array",
            "attribute",
            "comment",
            "document-node",
            "element",
            "empty-sequence",
            "function",
            "if",
            "item",
            "map",
            "namespace-node",
            // TODO: This one should be added back in but this breaks function calls like `node-name(.)`
            // "node",
            "processing-instruction",
            "schema-attribute",
            "schema-element",
            "switch",
            "text",
            "typeswitch"
        }.Select(Token).ToArray());

    public static readonly ParseFunc<Ast> LocationPathAbbreviation =
        Map(Token("//"), _ =>
            // TODO: convert descendant-or-self to enum
            new Ast(AstNodeName.StepExpr, new Ast(AstNodeName.XPathAxis)
                {
                    TextContent = "descendant-or-self"
                },
                new Ast(AstNodeName.AnyKindTest))
        );
}