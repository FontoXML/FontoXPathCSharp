using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public static class TypesParser
{
    public static ParseFunc<Ast> SingleType =
        Map(Token("AAAAAAAAAAA"), _ => new Ast(AstNodeName.All));
}