using System.Diagnostics;
using System.Xml;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Expressions.Axes;
using FontoXPathCSharp.Expressions.Operators;
using FontoXPathCSharp.Expressions.Tests;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Types.Node;
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

    private static AbstractTestExpression CompileElementTest(Ast ast)
    {
        var elementName = ast.GetFirstChild(AstNodeName.ElementName);
        var star = elementName?.GetFirstChild(AstNodeName.Star);
        if (elementName == null || star != null)
            return new KindTest(XmlNodeType.Element);
        return new NameTest(elementName.GetFirstChild(AstNodeName.QName).GetQName(), XmlNodeType.Element);
    }

    private static AbstractTestExpression CompileAttributeTest(Ast ast)
    {
        var attributeName = ast.GetFirstChild(AstNodeName.AttributeName);
        var star = attributeName?.GetFirstChild(AstNodeName.Star);
        if (attributeName == null || star != null)
            return new KindTest(XmlNodeType.Attribute);
        return new NameTest(attributeName.GetFirstChild(AstNodeName.QName).GetQName(), XmlNodeType.Attribute);
    }

    private static AbstractTestExpression CompileWildcard(Ast ast)
    {
        if (ast.GetFirstChild(AstNodeName.Star) == null)
            return new NameTest(new QName("*", null, "*"));

        var uri = ast.GetFirstChild(AstNodeName.Uri);
        if (uri != null)
            return new NameTest(new QName("*", uri.TextContent, ""));

        var ncName = ast.GetFirstChild(AstNodeName.NcName);
        Debug.Assert(ncName != null, nameof(ncName) + " != null");

        var firstChild = ast.GetFirstChild();
        Debug.Assert(firstChild != null, nameof(firstChild) + " != null");

        return firstChild.IsA(AstNodeName.Star)
            ? new NameTest(new QName(ncName.TextContent, null, "*"))
            : new NameTest(new QName("*", null, ncName.TextContent));
    }

    private static AbstractTestExpression CompileTestExpression(Ast ast)
    {
        return ast.Name switch
        {
            AstNodeName.NameTest => new NameTest(new QName(ast.TextContent, null, null)),
            AstNodeName.AnyKindTest => new TypeTest(new QName("node()", null, "")),
            AstNodeName.AttributeTest => CompileAttributeTest(ast),
            AstNodeName.ElementTest => CompileElementTest(ast),
            AstNodeName.Wildcard => CompileWildcard(ast),
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

            AbstractExpression stepExpression;

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
                    "ancestor" => new AncestorAxis(testExpression, false),
                    "ancestor-or-self" => new AncestorAxis(testExpression, true),
                    "descendant" => new DescendantAxis(testExpression, false),
                    "descendant-or-self" => new DescendantAxis(testExpression, true),
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
                stepExpression = CompileAst(filterExpr, DisallowUpdating(options));
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

        Debug.Assert(firstOperand != null, nameof(firstOperand) + " != null");
        var firstExpression = CompileAst(firstOperand, options);

        Debug.Assert(secondOperand != null, nameof(secondOperand) + " != null");
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
            AstNodeName.SequenceExpr => CompileSequenceExpression(ast, options),
            AstNodeName.UnionOp => CompileUnionOp(ast, options),
            _ => throw new InvalidDataException(ast.Name.ToString())
        };
    }

    private static AbstractExpression CompileUnionOp(Ast ast, CompilationOptions options)
    {
        return new UnionOperator(new[]
        {
            CompileAst(ast.FollowPath(AstNodeName.FirstOperand, AstNodeName.All)!, options),
            CompileAst(ast.FollowPath(AstNodeName.SecondOperand, AstNodeName.All)!, options)
        });
    }

    private static AbstractExpression CompileSequenceExpression(Ast ast, CompilationOptions options)
    {
        var childExpressions = ast.GetChildren(AstNodeName.All).Select(arg => CompileAst(arg, options)).ToArray();
        if (childExpressions.Length == 1) return childExpressions.First();

        return new SequenceExpression(childExpressions);
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