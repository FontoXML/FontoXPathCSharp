using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp;

public static class CompileAstToExpression
{
    private static AbstractTestExpression CompileTestExpression(Ast ast)
    {
        return ast.Name switch
        {
            "nameTest" => new NameTest(new QName(ast.TextContent, null, null)),
            _ => throw new InvalidDataException(ast.Name)
        };
    }

    private static AbstractExpression CompilePathExpression(Ast ast)
    {
        var steps = ast.GetChildren("stepExpr").Select<Ast, AbstractExpression>(step =>
        {
            var axis = step.GetFirstChild("xpathAxis");

            if (axis == null)
                throw new NotImplementedException();

            var test = step.GetFirstChild(new[]
            {
                "attributeTest",
                "anyElementTest",
                "piTest",
                "documentTest",
                "elementTest",
                "commentTest",
                "namespaceTest",
                "anyKindTest",
                "textTest",
                "anyFunctionTest",
                "typedFunctionTest",
                "schemaAttributeTest",
                "atomicType",
                "anyItemType",
                "parenthesizedItemType",
                "typedMapTest",
                "typedArrayTest",
                "nameTest",
                "Wildcard"
            });

            if (test == null)
                throw new InvalidOperationException("No test found in path expression axis");

            var testExpression = CompileTestExpression(test);

            return axis.TextContent switch
            {
                "self" => new SelfAxis(testExpression),
                "parent" => new ParentAxis(testExpression),
                _ => throw new NotImplementedException()
            };
        });

        return new PathExpression(steps.ToArray());
    }

    private static AbstractExpression CompileFunctionCallExpression(Ast ast)
    {
        var functionName = ast.GetFirstChild("functionName");
        if (functionName == null)
            throw new InvalidDataException(ast.Name);

        var args = ast.GetFirstChild("arguments")?.GetChildren("*");
        if (args == null)
            throw new InvalidDataException($"Missing args for {ast}");

        var argExpressions = args.Select(CompileAst).ToArray();

        return new FunctionCall(new NamedFunctionRef(functionName.GetQName(), args.Count()), argExpressions);
    }

    public static AbstractExpression CompileAst(Ast ast)
    {
        return ast.Name switch
        {
            "pathExpr" => CompilePathExpression(ast),
            "functionCallExpr" => CompileFunctionCallExpression(ast),
            _ => throw new InvalidDataException(ast.Name)
        };
    }
}