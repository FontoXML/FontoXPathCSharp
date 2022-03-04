using FontoXPathCSharp.Expressions;

namespace FontoXPathCSharp;

public class CompileAstToExpression
{
    public static AbstractTestExpression CompileTest(Ast ast)
    {
        if (ast.Name == "nameTest")
        {
            var name = ast.TextContent;
            return new NameTest(new QName(name!, null, null));
        }

        throw new InvalidDataException(ast.Name);
    }

    public static Expression CompileAst(Ast ast)
    {
        switch (ast.Name)
        {
            case "pathExpr":
            {
                var steps = new List<Expression>();

                return new PathExpression(steps.ToArray());
            }
        }

        throw new InvalidDataException(ast.Name);
    }
}