using System.Diagnostics;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Expressions.Axes;
using FontoXPathCSharp.Expressions.Operators;
using FontoXPathCSharp.Expressions.Operators.Compares;
using FontoXPathCSharp.Expressions.Tests;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public static class CompileAstToExpression<TNode> where TNode : notnull
{
    private static CompilationOptions DisallowUpdating(CompilationOptions options)
    {
        if (!options.AllowXQuery) return CompilationOptions.XPathMode;
        if (!options.AllowUpdating) return CompilationOptions.XQueryMode;
        return CompilationOptions.XQueryUpdatingMode;
    }

    private static AbstractTestExpression<TNode> CompileElementTest(Ast ast)
    {
        var elementName = ast.GetFirstChild(AstNodeName.ElementName);
        var star = elementName?.GetFirstChild(AstNodeName.Star);
        if (elementName == null || star != null)
            return new KindTest<TNode>(NodeType.Element);
        return new NameTest<TNode>(elementName.GetFirstChild(AstNodeName.QName)!.GetQName(), NodeType.Element);
    }

    private static AbstractTestExpression<TNode> CompileAttributeTest(Ast ast)
    {
        var attributeName = ast.GetFirstChild(AstNodeName.AttributeName);
        var star = attributeName?.GetFirstChild(AstNodeName.Star);
        if (attributeName == null || star != null)
            return new KindTest<TNode>(NodeType.Attribute);
        return new NameTest<TNode>(attributeName.GetFirstChild(AstNodeName.QName)!.GetQName(), NodeType.Attribute);
    }

    private static AbstractTestExpression<TNode> CompileWildcard(Ast ast)
    {
        if (ast.GetFirstChild(AstNodeName.Star) == null)
            return new NameTest<TNode>(new QName("*", null, "*"));

        var uri = ast.GetFirstChild(AstNodeName.Uri);
        if (uri != null)
            return new NameTest<TNode>(new QName("*", uri.TextContent, ""));

        var ncName = ast.GetFirstChild(AstNodeName.NcName);
        Debug.Assert(ncName != null, nameof(ncName) + " != null");

        var firstChild = ast.GetFirstChild();
        Debug.Assert(firstChild != null, nameof(firstChild) + " != null");

        return firstChild.IsA(AstNodeName.Star)
            ? new NameTest<TNode>(new QName(ncName.TextContent, null, "*"))
            : new NameTest<TNode>(new QName("*", null, ncName.TextContent));
    }

    private static AbstractTestExpression<TNode> CompileTestExpression(Ast ast)
    {
        return ast.Name switch
        {
            AstNodeName.NameTest => new NameTest<TNode>(new QName(ast.TextContent)),
            AstNodeName.AnyKindTest => CompileTypeTest(ast),
            AstNodeName.AttributeTest => CompileAttributeTest(ast),
            AstNodeName.ElementTest => CompileElementTest(ast),
            AstNodeName.CommentTest => CompileCommentTest(),
            AstNodeName.Wildcard => CompileWildcard(ast),
            AstNodeName.TextTest => CompileTextTest(),
            AstNodeName.AtomicType => CompileTypeTest(ast),
            _ => throw new NotImplementedException($"{ast.Name} AST to Expression not yet implemented")
        };
    }

    private static AbstractTestExpression<TNode> CompileCommentTest()
    {
        return new KindTest<TNode>(NodeType.Comment);
    }

    private static AbstractTestExpression<TNode> CompileTypeTest(Ast ast)
    {
        return new TypeTest<TNode>(ast.GetQName());
    }

    private static AbstractTestExpression<TNode> CompileTextTest()
    {
        return new KindTest<TNode>(NodeType.Text);
    }

    private static AbstractExpression<TNode> CompilePathExpression(Ast ast, CompilationOptions options)
    {
        var rawSteps = ast.GetChildren(AstNodeName.StepExpr).ToArray();
        var hasAxisStep = false;

        var steps = rawSteps.Select(step =>
        {
            var axis = step.GetFirstChild(AstNodeName.XPathAxis);

            var children = step.GetChildren(AstNodeName.All);
            var postFixExpressions = new List<(string Type, AbstractExpression<TNode> Postfix)>();

            string? intersectingBucket = null;
            foreach (var child in children)
                switch (child.Name)
                {
                    case AstNodeName.Lookup:
                        postFixExpressions.Add(("lookup", CompileLookup(child, options)));
                        break;
                    case AstNodeName.Predicate:
                    case AstNodeName.Predicates:
                        foreach (var childPredicate in child.GetChildren(AstNodeName.All))
                        {
                            var predicateExpression = CompileAst(
                                childPredicate,
                                DisallowUpdating(options)
                            );
                            intersectingBucket = BucketUtils.IntersectBuckets(
                                intersectingBucket,
                                predicateExpression.GetBucket()
                            );

                            postFixExpressions.Add(("predicate", predicateExpression));
                        }

                        break;
                }

            AbstractExpression<TNode> stepExpression;

            if (axis != null)
            {
                hasAxisStep = true;
                var test = step.GetFirstChild(
                    AstNodeName.AttributeTest, 
                    AstNodeName.AnyElementTest, 
                    AstNodeName.PiTest,
                    AstNodeName.DocumentTest, 
                    AstNodeName.ElementTest, 
                    AstNodeName.CommentTest,
                    AstNodeName.NamespaceTest,
                    AstNodeName.AnyKindTest, 
                    AstNodeName.TextTest, 
                    AstNodeName.AnyFunctionTest,
                    AstNodeName.TypedFunctionTest, 
                    AstNodeName.SchemaAttributeTest, 
                    AstNodeName.AtomicType,
                    AstNodeName.AnyItemType, 
                    AstNodeName.ParenthesizedItemType, 
                    AstNodeName.TypedMapTest,
                    AstNodeName.TypedArrayTest, 
                    AstNodeName.NameTest, 
                    AstNodeName.Wildcard
                );
                
                if (test == null) throw new Exception("No test found in path expression axis");

                var testExpression = CompileTestExpression(test);

                stepExpression = axis.TextContent switch
                {
                    "self" => new SelfAxis<TNode>(testExpression, intersectingBucket),
                    "parent" => new ParentAxis<TNode>(testExpression, intersectingBucket),
                    "child" => new ChildAxis<TNode>(testExpression, intersectingBucket),
                    "attribute" => new AttributeAxis<TNode>(testExpression, intersectingBucket),
                    "ancestor" => new AncestorAxis<TNode>(testExpression, false),
                    "ancestor-or-self" => new AncestorAxis<TNode>(testExpression, true),
                    "descendant" => new DescendantAxis<TNode>(testExpression, false),
                    "descendant-or-self" => new DescendantAxis<TNode>(testExpression, true),
                    "following" => new FollowingAxis<TNode>(testExpression),
                    "preceding" => new PrecedingAxis<TNode>(testExpression),
                    "following-sibling" => new FollowingSiblingAxis<TNode>(testExpression, intersectingBucket),
                    "preceding-sibling" => new PrecedingSiblingAxis<TNode>(testExpression, intersectingBucket),
                    _ => throw new InvalidDataException("Unknown axis type '" + axis.TextContent + "'")
                };
            }
            else
            {
                var filterExpr = step.FollowPath(AstNodeName.FilterExpr, AstNodeName.All);
                stepExpression = CompileAst(filterExpr!, DisallowUpdating(options));
            }

            foreach (var postfix in postFixExpressions)
                stepExpression = postfix.Type switch
                {
                    "lookup" => throw new NotImplementedException(
                        "CompileAstToExpression.CompilePathExpression lookup postfix expression not implemented yet."),
                    "predicate" => new FilterExpression<TNode>(stepExpression, postfix.Postfix),
                    _ => throw new Exception("Unreachable")
                };

            return stepExpression;
        }).ToArray();

        var isAbsolute = ast.GetFirstChild(AstNodeName.RootExpr);

        var requireSorting = hasAxisStep || isAbsolute != null || rawSteps.Length > 1;

        // Directly use expressions which are not path expression
        if (!requireSorting && steps.Length == 1) return steps[0];

        return new PathExpression<TNode>(steps.ToArray(), requireSorting);
    }

    private static AbstractExpression<TNode> CompileLookup(Ast ast, CompilationOptions options)
    {
        var keyExpression = ast.GetFirstChild();
        if (keyExpression == null) throw new Exception("Lookup did not contain a key expression.");
        return keyExpression.Name switch
        {
            AstNodeName.NcName => new Literal<TNode>(keyExpression.TextContent,
                new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)),
            AstNodeName.Star => throw new NotImplementedException("Star in Lookup is not implemented yet."),
            _ => CompileAst(keyExpression, DisallowUpdating(options))
        };
    }

    private static AbstractExpression<TNode> CompileFunctionCallExpression(Ast ast, CompilationOptions options)
    {
        var functionName = ast.GetFirstChild(AstNodeName.FunctionName);
        if (functionName == null)
            throw new InvalidDataException(ast.Name.ToString());

        var args = ast.GetFirstChild(AstNodeName.Arguments)?.GetChildren(AstNodeName.All);
        if (args == null)
            throw new InvalidDataException($"Missing args for {ast}");

        args = args.ToList();
        var argExpressions = args.Select(arg => CompileAst(arg, options)).ToArray();

        return new FunctionCall<TNode>(new NamedFunctionRef<TNode>(functionName.GetQName(), args.Count()),
            argExpressions);
    }

    private static AbstractExpression<TNode> CompileIntegerConstantExpression(Ast ast)
    {
        return new Literal<TNode>(ast.GetFirstChild(AstNodeName.Value)!.TextContent,
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne));
    }

    private static AbstractExpression<TNode> CompileStringConstantExpr(Ast ast)
    {
        return new Literal<TNode>(ast.GetFirstChild(AstNodeName.Value)!.TextContent,
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne));
    }

    private static AbstractExpression<TNode> CompileDoubleConstantExpr(Ast ast)
    {
        return new Literal<TNode>(ast.GetFirstChild(AstNodeName.Value)!.TextContent,
            new SequenceType(ValueType.XsDouble, SequenceMultiplicity.ExactlyOne));
    }

    private static AbstractExpression<TNode> CompileDecimalConstantExpr(Ast ast)
    {
        return new Literal<TNode>(ast.GetFirstChild(AstNodeName.Value)!.TextContent,
            new SequenceType(ValueType.XsDecimal, SequenceMultiplicity.ExactlyOne));
    }

    private static AbstractExpression<TNode> CompileStringConcatenateExpr(Ast ast, CompilationOptions options)
    {
        Console.WriteLine(ast);
        var args = new[]
        {
            ast.FollowPath(AstNodeName.FirstOperand, AstNodeName.All)!,
            ast.FollowPath(AstNodeName.SecondOperand, AstNodeName.All)!
        };
        Console.WriteLine(args[0]);
        return new FunctionCall<TNode>(new NamedFunctionRef<TNode>(
            new QName("concat", "http://www.w3.org/2005/xpath-functions", ""),
            args.Length
        ), args.Select(arg => CompileAst(arg, DisallowUpdating(options))).ToArray());
    }

    private static AbstractExpression<TNode> CompileCompareExpr(Ast ast, CompilationOptions options)
    {
        var firstOperand = ast.FollowPath(AstNodeName.FirstOperand, AstNodeName.All);
        var secondOperand = ast.FollowPath(AstNodeName.SecondOperand, AstNodeName.All);

        Debug.Assert(firstOperand != null, nameof(firstOperand) + " != null");
        var firstExpression = CompileAst(firstOperand, options);

        Debug.Assert(secondOperand != null, nameof(secondOperand) + " != null");
        var secondExpression = CompileAst(secondOperand, options);

        return ast.Name switch
        {
            AstNodeName.EqualOp => new GeneralCompare<TNode>(CompareType.Equal, firstExpression, secondExpression),
            AstNodeName.NotEqualOp =>
                new GeneralCompare<TNode>(CompareType.NotEqual, firstExpression, secondExpression),
            AstNodeName.LessThanOrEqualOp => new GeneralCompare<TNode>(CompareType.LessEquals, firstExpression,
                secondExpression),
            AstNodeName.LessThanOp =>
                new GeneralCompare<TNode>(CompareType.LessThan, firstExpression, secondExpression),
            AstNodeName.GreaterThanOrEqualOp => new GeneralCompare<TNode>(CompareType.GreaterEquals, firstExpression,
                secondExpression),
            AstNodeName.GreaterThanOp => new GeneralCompare<TNode>(CompareType.GreaterThan, firstExpression,
                secondExpression),
            AstNodeName.EqOp => new ValueCompare<TNode>(CompareType.Equal, firstExpression, secondExpression),
            AstNodeName.NeOp => new ValueCompare<TNode>(CompareType.NotEqual, firstExpression, secondExpression),
            AstNodeName.LtOp => new ValueCompare<TNode>(CompareType.LessThan, firstExpression, secondExpression),
            AstNodeName.LeOp => new ValueCompare<TNode>(CompareType.LessEquals, firstExpression, secondExpression),
            AstNodeName.GtOp => new ValueCompare<TNode>(CompareType.GreaterThan, firstExpression, secondExpression),
            AstNodeName.GeOp => new ValueCompare<TNode>(CompareType.GreaterEquals, firstExpression, secondExpression),
            _ => throw new Exception("Unreachable")
        };
    }

    public static AbstractExpression<TNode> CompileAst(Ast ast, CompilationOptions options)
    {
        return ast.Name switch
        {
            AstNodeName.Module => CompileModule(ast, options),
            AstNodeName.MainModule => CompileMainModule(ast, options),
            AstNodeName.QueryBody => CompileAst(ast.GetFirstChild()!, options),
            AstNodeName.PathExpr => CompilePathExpression(ast, options),
            AstNodeName.FunctionCallExpr => CompileFunctionCallExpression(ast, options),
            AstNodeName.ContextItemExpr => new ContextItemExpression<TNode>(),
            AstNodeName.IntegerConstantExpr => CompileIntegerConstantExpression(ast),
            AstNodeName.StringConstantExpr => CompileStringConstantExpr(ast),
            AstNodeName.DecimalConstantExpr => CompileDecimalConstantExpr(ast),
            AstNodeName.DoubleConstantExpr => CompileDoubleConstantExpr(ast),
            AstNodeName.VarRef => CompileVarRef(ast),
            AstNodeName.FlworExpr => CompileFlworExpr(ast, options),
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
            AstNodeName.OrOp => CompileOrOp(ast, options),
            AstNodeName.SequenceExpr => CompileSequenceExpression(ast, options),
            AstNodeName.UnionOp => CompileUnionOp(ast, options),
            AstNodeName.SubtractOp
                or AstNodeName.AddOp
                or AstNodeName.MultiplyOp
                or AstNodeName.DivOp
                or AstNodeName.IDivOp
                or AstNodeName.ModOp => CompileBinaryOperator(ast, options),
            AstNodeName.UnaryPlusOp or AstNodeName.UnaryMinusOp => CompileUnaryOperator(ast, options),
            AstNodeName.CastExpr => CastAs(ast, options),
            AstNodeName.IfThenElseExpr => CompileIfThenElseExpr(ast, options),
            AstNodeName.DynamicFunctionInvocationExpr => CompileDynamicFunctionInvocationExpr(ast, options),
            AstNodeName.ArrowExpr => CompileArrowExpr(ast, options),
            AstNodeName.RangeSequenceExpr => CompileRangeSequenceExpr(ast, options),
            AstNodeName.InstanceOfExpr => CompileInstanceOfExpr(ast, options),
            _ => CompileTestExpression(ast)
        };
    }

    private static AbstractExpression<TNode> CompileVarRef(Ast ast)
    {
        var qualifiedName = ast.GetFirstChild(AstNodeName.Name)?.GetQName();
        if (qualifiedName == null) throw new Exception("Variable reference does not have a QName associated with it");
        return new VarRef<TNode>(qualifiedName);
    }

    private static AbstractExpression<TNode> CompileFlworExpr(Ast ast, CompilationOptions options)
    {
        var clausesAndReturnClause = ast.GetChildren(AstNodeName.All).ToArray();
        var returnClauseExpression = clausesAndReturnClause.Last().GetFirstChild();

        // Return intermediate and initial clauses handling
        var clauses = clausesAndReturnClause[..^1].ToList();

        // We have to check if there are any intermediate clauses before compiling them.
        if (clauses.Count > 1)
            if (!options.AllowXQuery)
                throw new XPathException("XPST0003", "Use of XQuery FLWOR expressions in XPath is no allowed");

        // TODO: Turn this back into a ReduceRight.
        clauses.Reverse();
        if (returnClauseExpression == null) throw new Exception("Return clause in flwor expression was null");
        return clauses.Aggregate(
            CompileAst(returnClauseExpression, options),
            (returnOfPreviousExpression, flworExpressionClause) =>
            {
                return flworExpressionClause.Name switch
                {
                    AstNodeName.ForClause => throw new Exception(
                        $"Not implemented: {flworExpressionClause.Name} is not implemented yet."),
                    // return forClause(
                    //     flworExpressionClause,
                    //     compilationOptions,
                    //     returnOfPreviousExpression
                    // );
                    AstNodeName.LetClause => LetClause(flworExpressionClause, options,
                        returnOfPreviousExpression),
                    AstNodeName.WhereClause => throw new Exception(
                        $"Not implemented: {flworExpressionClause.Name} is not implemented yet."),
                    // return whereClause(
                    //     flworExpressionClause,
                    //     compilationOptions,
                    //     returnOfPreviousExpression
                    // );
                    AstNodeName.WindowClause => throw new Exception(
                        $"Not implemented: {flworExpressionClause.Name} is not implemented yet."),
                    AstNodeName.GroupByClause => throw new Exception(
                        $"Not implemented: {flworExpressionClause.Name} is not implemented yet."),
                    AstNodeName.OrderByClause => throw new Exception(
                        $"Not implemented: {flworExpressionClause.Name} is not implemented yet."),
                    // return orderByClause(
                    //     flworExpressionClause,
                    //     compilationOptions,
                    //     returnOfPreviousExpression
                    // );
                    AstNodeName.CountClause => throw new Exception(
                        $"Not implemented: {flworExpressionClause.Name} is not implemented yet."),
                    _ => throw new Exception(
                        $"Not implemented: {flworExpressionClause.Name} is not supported in a flwor expression")
                };
            }
        );
    }

    private static AbstractExpression<TNode> LetClause(
        Ast expressionClause,
        CompilationOptions compilationOptions,
        AbstractExpression<TNode> returnClauseExpression)
    {
        var letClauseItems = expressionClause.GetChildren(AstNodeName.All).ToArray();
        var returnExpr = returnClauseExpression;

        for (var i = letClauseItems.Length - 1; i >= 0; --i)
        {
            var letClauseItem = letClauseItems[i];
            var expression = letClauseItem.FollowPath(AstNodeName.LetExpr, AstNodeName.All);
            returnExpr = new LetExpression<TNode>(
                letClauseItem.FollowPath(AstNodeName.TypedVariableBinding, AstNodeName.VarName)!.GetQName(),
                CompileAst(expression!, DisallowUpdating(compilationOptions)),
                returnExpr
            );
        }

        return returnExpr;
    }

    private static AbstractExpression<TNode> CompileInstanceOfExpr(Ast ast, CompilationOptions options)
    {
        var expression = CompileAst(ast.FollowPath(AstNodeName.ArgExpr, AstNodeName.All)!, options);
        var sequenceType = ast.FollowPath(AstNodeName.SequenceType, AstNodeName.All);
        var occurrence = ast.FollowPath(AstNodeName.SequenceType, AstNodeName.OccurrenceIndicator);

        return new InstanceOfOperator<TNode>(expression, CompileAst(sequenceType!, DisallowUpdating(options)),
            occurrence?.TextContent ?? "");
    }

    private static AbstractExpression<TNode> CompileRangeSequenceExpr(Ast ast, CompilationOptions options)
    {
        var args = new[]
        {
            ast.FollowPath(AstNodeName.StartExpr, AstNodeName.All),
            ast.FollowPath(AstNodeName.EndExpr, AstNodeName.All)
        };

        var functionRef = new NamedFunctionRef<TNode>(
            new QName("to", "http://fontoxpath/operators", ""),
            args.Length
        );

        return new FunctionCall<TNode>(functionRef,
            args.Select(arg => CompileAst(arg!, DisallowUpdating(options))).ToArray());
    }

    private static AbstractExpression<TNode> CompileArrowExpr(Ast ast, CompilationOptions options)
    {
        var argExpr = ast.FollowPath(AstNodeName.ArgExpr, AstNodeName.All);
        // Each part an EQName, expression, or arguments passed to the previous part
        var parts = ast.GetChildren(AstNodeName.All).Skip(1).ToArray();

        IEnumerable<AbstractExpression<TNode>?> args = new List<AbstractExpression<TNode>?>
            { CompileAst(argExpr!, options) };

        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Name == AstNodeName.Arguments) continue;

            if (parts[i + 1].Name == AstNodeName.Arguments)
            {
                var functionArguments = parts[i + 1].GetChildren(AstNodeName.All);
                args = args.Concat(
                    functionArguments.Select(arg =>
                        arg.Name == AstNodeName.ArgumentPlaceholder ? null : CompileAst(arg, options))
                );
            }

            var abstractExpressions = args as AbstractExpression<TNode>?[] ?? args.ToArray();
            var func = parts[i].Name == AstNodeName.EqName
                ? new NamedFunctionRef<TNode>(parts[i].GetQName(), abstractExpressions.Length)
                : CompileAst(parts[i], DisallowUpdating(options));
            args = new List<AbstractExpression<TNode>?> { new FunctionCall<TNode>(func, abstractExpressions!) };
        }

        return args.First()!;
    }

    private static AbstractExpression<TNode> CompileDynamicFunctionInvocationExpr(Ast ast, CompilationOptions options)
    {
        var functionItemContent = ast.FollowPath(AstNodeName.FunctionItem, AstNodeName.All);
        var argumentsAst = ast.GetFirstChild(AstNodeName.Arguments);
        var args = Array.Empty<AbstractExpression<TNode>>();
        if (argumentsAst != null)
        {
            var functionArguments = argumentsAst.GetChildren(AstNodeName.All);
            args = functionArguments.Select(arg =>
                arg.Name == AstNodeName.ArgumentPlaceholder ? null : CompileAst(arg, options)
            ).OfType<AbstractExpression<TNode>>().ToArray();
        }

        return new FunctionCall<TNode>(CompileAst(functionItemContent!, options), args);
    }

    private static AbstractExpression<TNode> CompileIfThenElseExpr(Ast ast, CompilationOptions options)
    {
        return new IfExpression<TNode>(
            CompileAst(ast.FollowPath(AstNodeName.IfClause, AstNodeName.All)!, options),
            CompileAst(ast.FollowPath(AstNodeName.ThenClause, AstNodeName.All)!, options),
            CompileAst(ast.FollowPath(AstNodeName.ElseClause, AstNodeName.All)!, options)
        );
    }


    private static AbstractExpression<TNode> CastAs(Ast ast, CompilationOptions options)
    {
        var expression = CompileAst(ast.GetFirstChild(AstNodeName.ArgExpr)?.GetFirstChild()!,
            DisallowUpdating(options));

        var singleType = ast.GetFirstChild(AstNodeName.SingleType);
        var targetType = singleType!.GetFirstChild(AstNodeName.AtomicType)!.GetQName();
        var optional = singleType.GetFirstChild(AstNodeName.Optional) != null;

        return new CastAsOperator<TNode>(expression, targetType, optional);
    }


    private static AbstractExpression<TNode> CompileBinaryOperator(Ast ast, CompilationOptions options)
    {
        var kind = ast.Name;
        var a = CompileAst(ast.FollowPath(AstNodeName.FirstOperand, AstNodeName.All)!, DisallowUpdating(options));
        var b = CompileAst(ast.FollowPath(AstNodeName.SecondOperand, AstNodeName.All)!, DisallowUpdating(options));

        return new BinaryOperator<TNode>(kind, a, b);
    }

    private static AbstractExpression<TNode> CompileUnaryOperator(Ast ast, CompilationOptions options)
    {
        var operand = ast.FollowPath(AstNodeName.Operand, AstNodeName.All);
        return new UnaryOperator<TNode>(ast.Name, CompileAst(operand!, options));
    }

    private static AbstractExpression<TNode> CompileUnionOp(Ast ast, CompilationOptions options)
    {
        return new UnionOperator<TNode>(new[]
        {
            CompileAst(ast.FollowPath(AstNodeName.FirstOperand, AstNodeName.All)!, options),
            CompileAst(ast.FollowPath(AstNodeName.SecondOperand, AstNodeName.All)!, options)
        });
    }

    private static AbstractExpression<TNode> CompileSequenceExpression(Ast ast, CompilationOptions options)
    {
        var childExpressions = ast.GetChildren(AstNodeName.All).Select(arg => CompileAst(arg, options)).ToArray();
        if (childExpressions.Length == 1) return childExpressions.First();

        return new SequenceOperator<TNode>(childExpressions);
    }

    private static AbstractExpression<TNode> CompileAndOp(Ast ast, CompilationOptions options)
    {
        return new AndOperator<TNode>(UnwrapBinaryOperator(AstNodeName.AndOp, ast, DisallowUpdating(options)));
    }

    private static AbstractExpression<TNode> CompileOrOp(Ast ast, CompilationOptions options)
    {
        return new OrOperator<TNode>(UnwrapBinaryOperator(AstNodeName.OrOp, ast, DisallowUpdating(options)));
    }

    private static AbstractExpression<TNode>[] UnwrapBinaryOperator(AstNodeName operatorName, Ast ast,
        CompilationOptions options)
    {
        var compiledAstNodes = new List<AbstractExpression<TNode>>();

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

    private static AbstractExpression<TNode> CompileModule(Ast module, CompilationOptions options)
    {
        return CompileAst(module.GetFirstChild(AstNodeName.MainModule)!, options);
    }

    private static AbstractExpression<TNode> CompileMainModule(Ast mainModule, CompilationOptions options)
    {
        var prolog = mainModule.GetFirstChild(AstNodeName.Prolog);
        if (prolog != null) ProcessProlog(prolog);
        return CompileAst(mainModule.GetFirstChild(AstNodeName.QueryBody)!, options);
    }

    private static void ProcessProlog(Ast prolog)
    {
        throw new NotImplementedException($"ProcessProlog not implemented: {prolog}");
    }
}