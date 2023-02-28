using FontoXPathCSharp.Value;
using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public class TypesParser
{
    private readonly ParseFunc<QName> SimpleTypeName;

    public readonly ParseFunc<Ast> SingleType;

    public readonly ParseFunc<QName> TypeName;

    public TypesParser(NameParser nameParser)
    {
        TypeName = nameParser.EqName;

        SimpleTypeName = TypeName;

        SingleType = Then(
            SimpleTypeName,
            Optional(Token("?")),
            (type, opt) =>
                opt == null
                    ? new Ast(AstNodeName.SingleType, type.GetAst(AstNodeName.AtomicType))
                    : new Ast(AstNodeName.SingleType, type.GetAst(AstNodeName.AtomicType),
                        new Ast(AstNodeName.Optional))
        );
    }
}