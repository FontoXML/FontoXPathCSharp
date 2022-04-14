using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public class LiteralParser
{
    private static ParseFunc<ParseResult<string>> Digits()
    {
        return Regex(@"[0-9]+");
    }

    private static ParseFunc<ParseResult<Ast>> IntegerLiteral()
    {
        return Map(Digits(), d => new Ast(AstNodeName.IntegerConstantExpr)
        {
            StringAttributes =
            {
                ["value"] = d
            }
        });
    }

    public static ParseFunc<ParseResult<Ast>> NumericLiteral()
    {
        return Followed(
            Or(new[] {IntegerLiteral()}),
            Peek(Not(Regex(@"[a-z][A-Z]"), new[] {"No alphabetic characters after numeric literal"}))
        );
    }
}