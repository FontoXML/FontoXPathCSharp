using FontoXPathCSharp.Expressions;

namespace FontoXPathCSharp;

public class CompileAstToExpression
{
    public static AbstractTestAbstractExpression CompileTestExpression(Ast ast)
    {
        return ast.Name switch
        {
            "nameTest" => new NameTestAbstract(new QName(ast.TextContent!, null, null)),
            _ => throw new InvalidDataException(ast.Name)
        };
    }

    private static AbstractExpression CompilePathExpression(Ast ast)
    {
        var steps = ast.GetChildren("stepExpr").Select<Ast, AbstractExpression>(step =>
        {
            var axis = step.GetFirstChild("xpathAxis");

            if (axis != null)
            {
                var test = step.GetFirstChild(new[]
                {
                    // TODO: add others
                    "attributeTest",
                    "anyElementTest",
                    "piTest",
                    "documentTest",
                    "nameTest"
                });

                var testExpression = CompileTestExpression(test);
                switch (axis.TextContent)
                {
                    case "self":
                        return new SelfAxis(testExpression);
                }
            }
            
            throw new NotImplementedException();
        });

        return new PathExpression(steps.ToArray());
    }

    public static AbstractExpression CompileAst(Ast ast)
    {
        return ast.Name switch
        {
            "pathExpr" => CompilePathExpression(ast),
            _ => throw new InvalidDataException(ast.Name)
        };
    }
}