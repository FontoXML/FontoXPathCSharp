using FontoXPathCSharp.Value;
using PrscSharp;
using static PrscSharp.PrscSharp;
using static FontoXPathCSharp.Parsing.ParsingFunctions;
using static FontoXPathCSharp.Parsing.WhitespaceParser;
using Regex = System.Text.RegularExpressions.Regex;

namespace FontoXPathCSharp.Parsing;

public static class NameParser
{
    private static readonly ParseFunc<string> NcNameStartChar =
        Or(Regex(
                @"[A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD]"),
            Then(Regex(@"[\uD800-\uDB7F]"), Regex(@"[\uDC00-\uDFFF]"), (a, b) => a + b));

    private static readonly ParseFunc<string> NcNameChar =
        Or(NcNameStartChar, Regex(@"[\-\.0-9\xB7\u0300-\u036F\u203F\u2040]"));

    public static readonly ParseFunc<string> NcName =
        Then(
            NcNameStartChar,
            Star(NcNameChar),
            (a, b) => a + string.Join("", b)
        );

    private static readonly ParseFunc<QName> UnprefixedName =
        Map(NcName, x => new QName(x, null, ""));

    private static readonly ParseFunc<QName> QName =
        Or(
            UnprefixedName
        // TODO: add prefixed name
        );

    public static readonly ParseFunc<string> BracedUriLiteral = Followed(
        PrecededMultiple(
            new[] { Token("Q"), Whitespace, Token("{") },
            Map(Star(Regex("/[^{}]/")), x => Regex.Replace(string.Join("", x), @"\s+", " ").Trim())
        ),
        Token("}")
    );

    // TODO: add uriQualifiedName
    public static readonly ParseFunc<QName> EqName = Or(QName);

    public static readonly ParseFunc<QName> ElementName = EqName;

    public static readonly ParseFunc<Ast> ElementNameOrWildcard = Or(
        Map(ElementName, name => name.GetAst(AstNodeName.QName)),
        Map(Token("*"), _ => new Ast(AstNodeName.Star))
    );

    public static readonly ParseFunc<Ast> AttributeNameOrWildcard = ElementNameOrWildcard;

    private static readonly ParseFunc<QName> VarName = EqName;

    public static readonly ParseFunc<Ast> VarRef = Map(Preceded(Token("$"), VarName),
        x => new Ast(AstNodeName.VarRef, x.GetAst(AstNodeName.Name))
    );
}