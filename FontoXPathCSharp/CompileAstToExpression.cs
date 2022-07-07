using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Expressions.Axes;
using FontoXPathCSharp.Expressions.Operators;
using FontoXPathCSharp.Expressions.Tests;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public static class CompileAstToExpression
{
    private static CompilationOptions DisallowUpdating(CompilationOptions options)
    {
        // TODO: implement
        return options;
    }

    private static AbstractTestExpression CompileTestExpression(Ast ast)
    {
        return ast.Name switch
        {
            AstNodeName.NameTest => new NameTest(new QName(ast.TextContent, null, null)),
            AstNodeName.AnyKindTest => new TypeTest(new QName("node()", null, "")),
            _ => throw new XPathException("Invalid test expression: " + ast.Name)
        };
    }

    private static AbstractExpression CompilePathExpression(Ast ast, CompilationOptions options)
    {
        var rawSteps = ast.GetChildren(AstNodeName.StepExpr);

        var steps = rawSteps.Select<Ast, AbstractExpression>(step =>
        {
            var axis = step.GetFirstChild(AstNodeName.XPathAxis);

            var children = step.GetChildren(AstNodeName.All);
            var postFixExpressions = new List<(string Type, AbstractExpression Postfix)>();

            foreach (var child in children)
            {
                switch (child.Name)
                {
                    case AstNodeName.Lookup:
                        throw new NotImplementedException("CompileAstToExpression Lookup");
                    case AstNodeName.Predicate:
                    case AstNodeName.Predicates:
                        postFixExpressions
                            .AddRange(child.GetChildren(AstNodeName.All)
                                .Select(childPredicate => CompileAst(childPredicate, DisallowUpdating(options)))
                                .Select(predicateExpression => ("predicate", predicateExpression)));
                        break;
                }
            }

            AbstractExpression? stepExpression = null;

            if (axis != null)
            {
                var test = step.GetFirstChild(
                    AstNodeName.AttributeTest, AstNodeName.AnyElementTest, AstNodeName.PiTest,
                    AstNodeName.DocumentTest, AstNodeName.ElementTest, AstNodeName.CommentTest,
                    AstNodeName.NamespaceTest,
                    AstNodeName.AnyKindTest, AstNodeName.TextTest, AstNodeName.AnyFunctionTest,
                    AstNodeName.TypedFunctionTest, AstNodeName.SchemaAttributeTest, AstNodeName.AtomicType,
                    AstNodeName.AnyItemType, AstNodeName.ParenthesizedItemType, AstNodeName.TypedMapTest,
                    AstNodeName.TypedArrayTest, AstNodeName.NameTest, AstNodeName.Wildcard);

                if (test == null)
                {
                    throw new XPathException("No test found in path expression axis");
                }

                var testExpression = CompileTestExpression(test);

                stepExpression = axis.TextContent switch
                {
                    "self" => new SelfAxis(testExpression),
                    "parent" => new ParentAxis(testExpression),
                    "child" => new ChildAxis(testExpression),
                    "attribute" => new AttributeAxis(testExpression),
                    "ancestor" => new AncestorAxis(testExpression),
                    "descendant" => new DescendantAxis(testExpression),
                    "following" => new FollowingAxis(testExpression),
                    "preceding" => new PrecedingAxis(testExpression),
                    "following-sibling" => new FollowingSiblingAxis(testExpression),
                    "preceding-sibling" => new PrecedingSiblingAxis(testExpression),
                    _ => throw new InvalidDataException("Unknown axis type '" + axis.TextContent + "'")
                };
            }
            else
            {
                var filterExpr = step.FollowPath(AstNodeName.FilterExpr, AstNodeName.All);
                if (filterExpr != null) stepExpression = CompileAst(filterExpr, DisallowUpdating(options));
            }

            foreach (var postfix in postFixExpressions)
            {
                stepExpression = postfix.Type switch
                {
                    "lookup" => throw new NotImplementedException(),
                    "predicate" => new FilterExpression(stepExpression, postfix.Postfix),
                    _ => throw new Exception("Unreachable")
                };
            }

            return stepExpression;
        });

        return new PathExpression(steps.ToArray());
    }

    private static AbstractExpression CompileFunctionCallExpression(Ast ast, CompilationOptions options)
    {
        var functionName = ast.GetFirstChild(AstNodeName.FunctionName);
        if (functionName == null)
            throw new InvalidDataException(ast.Name.ToString());

        var args = ast.GetFirstChild(AstNodeName.Arguments)?.GetChildren(AstNodeName.All);
        if (args == null)
            throw new InvalidDataException($"Missing args for {ast}");

        args = args.ToList();
        var argExpressions = args.Select(arg => CompileAst(arg, options)).ToArray();

        return new FunctionCall(new NamedFunctionRef(functionName.GetQName(), args.Count()), argExpressions);
    }

    private static AbstractExpression CompileIntegerConstantExpression(Ast ast)
    {
        return new Literal(ast.GetFirstChild(AstNodeName.Value)!.TextContent,
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne));
    }

    private static AbstractExpression CompileStringConstantExpr(Ast ast)
    {
        return new Literal(ast.GetFirstChild(AstNodeName.Value)!.TextContent,
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne));
    }

    private static AbstractExpression CompileStringConcatenateExpr(Ast ast, CompilationOptions options)
    {
        Console.WriteLine(ast);
        var args = new[]
        {
            ast.FollowPath(AstNodeName.FirstOperand, AstNodeName.All)!,
            ast.FollowPath(AstNodeName.SecondOperand, AstNodeName.All)!,
        };
        Console.WriteLine(args[0]);
        return new FunctionCall(new NamedFunctionRef(
            new QName("concat", "http://www.w3.org/2005/xpath-functions", ""),
            args.Length
        ), args.Select(arg => CompileAst(arg, DisallowUpdating(options))).ToArray());
    }

    private static AbstractExpression CompileCompareExpr(Ast ast, CompilationOptions options)
    {
        var firstOperand = ast.FollowPath(AstNodeName.FirstOperand, AstNodeName.All);
        var secondOperand = ast.FollowPath(AstNodeName.SecondOperand, AstNodeName.All);
        
        var firstExpression = CompileAst(firstOperand, options);
        var secondExpression = CompileAst(secondOperand, options);

        return ast.Name switch
        {
            AstNodeName.EqualOp =>
                new GeneralCompare(CompareType.Equal, firstExpression, secondExpression),
            AstNodeName.NotEqualOp =>
                new GeneralCompare(CompareType.NotEqual, firstExpression, secondExpression),
            AstNodeName.LessThanOrEqualOp
                or AstNodeName.LessThanOp
                or AstNodeName.GreaterThanOrEqualOp
                or AstNodeName.GreaterThanOp => throw new NotImplementedException(
                    "CompileAstToExpression: Other general compare operators"),
            AstNodeName.EqOp =>
                new ValueCompare(CompareType.Equal, firstExpression, secondExpression),
            AstNodeName.NeOp =>
                new ValueCompare(CompareType.NotEqual, firstExpression, secondExpression),
            AstNodeName.LtOp
                or AstNodeName.LeOp
                or AstNodeName.GtOp
                or AstNodeName.GeOp => throw new NotImplementedException(
                    "CompileAstToExpression: Other value compare operators"),
            _ => throw new Exception("Unreachable")
        };
    }

    public static AbstractExpression CompileAst(Ast ast, CompilationOptions options)
    {
        return ast.Name switch
        {
            AstNodeName.Module => CompileModule(ast, options),
            AstNodeName.MainModule => CompileMainModule(ast, options),
            AstNodeName.QueryBody => CompileAst(ast.GetFirstChild()!, options),
            AstNodeName.PathExpr => CompilePathExpression(ast, options),
            AstNodeName.FunctionCallExpr => CompileFunctionCallExpression(ast, options),
            AstNodeName.IntegerConstantExpr => CompileIntegerConstantExpression(ast),
            AstNodeName.ContextItemExpr => new ContextItemExpression(),
            AstNodeName.StringConstantExpr => CompileStringConstantExpr(ast),
            AstNodeName.StringConcatenateOp => CompileStringConcatenateExpr(ast, options),
            AstNodeName.EqualOp
                or AstNodeName.NotEqualOp
                or AstNodeName.LessThanOrEqualOp
                or AstNodeName.LessThanOp
                or AstNodeName.GreaterThanOrEqualOp
                or AstNodeName.GreaterThanOp => CompileCompareExpr(ast, options),
            AstNodeName.EqOp
                or AstNodeName.NeOp
                or AstNodeName.LtOp
                or AstNodeName.LeOp
                or AstNodeName.GtOp
                or AstNodeName.GeOp => CompileCompareExpr(ast, options),
            AstNodeName.AndOp => CompileAndOp(ast, options),
            _ => throw new InvalidDataException(ast.Name.ToString())
        };
    }

    private static AbstractExpression CompileAndOp(Ast ast, CompilationOptions options)
    {
        return new AndOperator(UnwrapBinaryOperator(AstNodeName.AndOp, ast, DisallowUpdating(options)));
    }

    private static AbstractExpression[] UnwrapBinaryOperator(AstNodeName operatorName, Ast ast,
        CompilationOptions options)
    {
        var compiledAstNodes = new List<AbstractExpression>();

        Action<Ast>? unwrapInner = null;
        unwrapInner = innerAst =>
        {
            var firstOperand = innerAst.GetFirstChild(AstNodeName.FirstOperand)?.GetFirstChild();
            var secondOperand = innerAst.GetFirstChild(AstNodeName.SecondOperand)?.GetFirstChild();

            if (firstOperand != null && firstOperand.Name == operatorName) unwrapInner?.Invoke(firstOperand);
            else if (firstOperand != null) compiledAstNodes.Add(CompileAst(firstOperand, options));

            if (secondOperand != null && secondOperand.Name == operatorName) unwrapInner?.Invoke(secondOperand);
            else if (secondOperand != null) compiledAstNodes.Add(CompileAst(secondOperand, options));
        };

        unwrapInner(ast);

        return compiledAstNodes.ToArray();
    }

    private static AbstractExpression CompileModule(Ast module, CompilationOptions options)
    {
        return CompileAst(module.GetFirstChild(AstNodeName.MainModule)!, options);
    }

    private static AbstractExpression CompileMainModule(Ast mainModule, CompilationOptions options)
    {
        var prolog = mainModule.GetFirstChild(AstNodeName.Prolog);
        if (prolog != null) ProcessProlog(prolog);
        return CompileAst(mainModule.GetFirstChild(AstNodeName.QueryBody)!, options);
    }

    private static void ProcessProlog(Ast prolog)
    {
        throw new NotImplementedException();
    }
}