using FontoXPathCSharp.Value;
using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public static class TypesParser
{
    public static readonly ParseFunc<QName> TypeName = NameParser.EqName;

    private static readonly ParseFunc<QName> SimpleTypeName = TypeName;

    public static readonly ParseFunc<Ast> SingleType = Then(
        SimpleTypeName,
        Optional(Token("?")),
        (type, opt) =>
            opt == null
                ? new Ast(AstNodeName.SingleType, type.GetAst(AstNodeName.AtomicType))
                : new Ast(AstNodeName.SingleType, type.GetAst(AstNodeName.AtomicType), new Ast(AstNodeName.Optional))
    );
}