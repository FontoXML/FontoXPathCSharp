using FontoXPathCSharp.Value;
using PrscSharp;
using static PrscSharp.PrscSharp;
using static FontoXPathCSharp.Parsing.ParsingFunctions;
using Regex = System.Text.RegularExpressions.Regex;

namespace FontoXPathCSharp.Parsing;

public class NameParser
{
    public readonly ParseFunc<Ast> AttributeNameOrWildcard;

    public readonly ParseFunc<string> BracedUriLiteral;

    public readonly ParseFunc<QName> ElementName;

    public readonly ParseFunc<Ast> ElementNameOrWildcard;

    public readonly ParseFunc<QName> EqName;

    private readonly ParseFunc<string> LocalPart;

    public readonly ParseFunc<string> NcName;

    private readonly ParseFunc<string> NcNameChar;

    private readonly ParseFunc<string> NcNameStartChar;

    private readonly ParseFunc<QName> PrefixedName;

    private readonly ParseFunc<QName> QName;

    private readonly ParseFunc<QName> UnprefixedName;

    public readonly ParseFunc<QName> UriQualifiedName;

    private readonly ParseFunc<QName> VarName;

    public readonly ParseFunc<Ast> VarRef;

    private readonly ParseFunc<string> XmlPrefix;

    public NameParser(WhitespaceParser whitespaceParser)
    {
        NcNameStartChar = Or(
            Regex(
                @"[A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD]"),
            Then(Regex(@"[\uD800-\uDB7F]"),
                Regex(@"[\uDC00-\uDFFF]"),
                (a, b) => a + b
            )
        );

        NcNameChar = Or(
            NcNameStartChar,
            Regex(@"[\-\.0-9\xB7\u0300-\u036F\u203F\u2040]")
        );

        NcName = Then(
            NcNameStartChar,
            Star(NcNameChar),
            (a, b) => a + string.Join("", b)
        );

        UnprefixedName = Map(NcName, x => new QName(x, null, ""));

        XmlPrefix = NcName;

        LocalPart = NcName;

        PrefixedName = Then(
            XmlPrefix,
            Preceded(Token(":"), LocalPart),
            (prefix, local) => new QName(local, null, prefix)
        );

        QName = Or(
            PrefixedName,
            UnprefixedName
        );

        BracedUriLiteral = Followed(
            PrecededMultiple(
                new[] { Token("Q"), whitespaceParser.Whitespace, Token("{") },
                Map(Star(Regex(@"[^{}]")), x => Regex.Replace(string.Join("", x), @"\s+", " ").Trim())
            ),
            Token("}")
        );

        UriQualifiedName = Then(
            BracedUriLiteral,
            NcName,
            (uri, localName) => new QName(localName, uri)
        );

        EqName = Or(UriQualifiedName, QName);

        ElementName = EqName;

        ElementNameOrWildcard = Or(
            Map(ElementName, name => name.GetAst(AstNodeName.QName)),
            Map(Token("*"), _ => new Ast(AstNodeName.Star))
        );

        AttributeNameOrWildcard = ElementNameOrWildcard;

        VarName = EqName;

        VarRef = Map(Preceded(Token("$"), VarName),
            x => new Ast(AstNodeName.VarRef, x.GetAst(AstNodeName.Name))
        );
    }
}