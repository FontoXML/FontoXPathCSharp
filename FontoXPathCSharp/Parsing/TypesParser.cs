using FontoXPathCSharp.Value;
using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public static class TypesParser
{
    public static readonly ParseFunc<QName> TypeName = NameParser.EqName;

    public static readonly ParseFunc<Ast> SingleType =
        Map(Token("TODO"), _ => new Ast(AstNodeName.All));
}