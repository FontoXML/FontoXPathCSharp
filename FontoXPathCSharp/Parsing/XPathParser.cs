using PrscSharp;
using static PrscSharp.PrscSharp;
using static FontoXPathCSharp.Parsing.NameParser;
using static FontoXPathCSharp.Parsing.ParsingFunctions;
using static FontoXPathCSharp.Parsing.WhitespaceParser;
using static FontoXPathCSharp.Parsing.LiteralParser;
using static FontoXPathCSharp.Parsing.TypesParser;

namespace FontoXPathCSharp.Parsing;

public static class XPathParser
{
    private static readonly ParseFunc<Ast> Predicate =
        Delimited(Token("["), Surrounded(Expr(), Whitespace), Token("]"));

    // TODO: add xquery string literal
    private static readonly ParseFunc<string> StringLiteral = Map(
        Or(Surrounded(Star(Or(Regex("[^\"]"))), Token("\"")),
            Surrounded(Star(Or(Regex("[^']"))), Token("'"))),
        x => string.Join("", x)
    );

    // TODO: add wildcard
    private static readonly ParseFunc<Ast> NameTest =
        Map(EqName, x => new Ast(AstNodeName.NameTest)
        {
            StringAttributes =
            {
                ["URI"] = x.NamespaceUri!,
                ["prefix"] = x.Prefix
            },
            TextContent = x.LocalName
        });

    // TODO: add kindTest
    private static readonly ParseFunc<Ast> NodeTest = Or(NameTest);

    private static readonly ParseFunc<Ast> ForwardStep
        = Or(Then(ForwardAxis, NodeTest,
            (axis, test) =>
                new Ast(AstNodeName.StepExpr,
                    new Ast(AstNodeName.XPathAxis) {TextContent = axis},
                    test
                )));

    // TODO: add predicateList
    // TODO: add reverse step
    private static readonly ParseFunc<Ast> AxisStep =
        Or(ForwardStep);

    // TODO: add string literal
    private static readonly ParseFunc<Ast> Literal =
        Or(NumericLiteral, Map(StringLiteral, x => new Ast(AstNodeName.StringConstantExpr, new Ast(AstNodeName.Value)
            {
                TextContent = x
            })
        ));

    // TODO: add argumentPlaceholder
    private static readonly ParseFunc<Ast> Argument =
        ExprSingle;

    private static readonly ParseFunc<Ast[]> ArgumentList =
        Map(
            Delimited(
                Token("("),
                Surrounded(
                    Optional(
                        Then(Argument,
                            Star(Preceded(Surrounded(Token(","), Whitespace), Argument)),
                            (first, following) => following.Prepend(first).ToArray())
                    ),
                    Whitespace
                ),
                Token(")")
            ),
            x => x ?? Array.Empty<Ast>()
        );

    private static readonly ParseFunc<Ast> FunctionCall =
        Preceded(
            Not(Followed(ReservedFunctionNames, new[] {Whitespace, Token("(")}),
                new[] {"cannot use reserved keyword for function names"}),
            Then(EqName, Preceded(Whitespace, ArgumentList),
                (name, arguments) =>
                {
                    var argumentsAst = new Ast(AstNodeName.Arguments, arguments);

                    var ast = new Ast(AstNodeName.FunctionCallExpr,
                        name.GetAst(AstNodeName.FunctionName),
                        argumentsAst
                    );
                    return ast;
                }
            )
        );

    private static readonly ParseFunc<Ast> SequenceType =
        Map(Token("AAAAAAAAAAA"), _ => new Ast(AstNodeName.All));

    // TODO: add others
    private static readonly ParseFunc<Ast> PrimaryExpr = Or(Literal, ContextItemExpr, FunctionCall);

    private static readonly ParseFunc<Ast> PostfixExprWithStep =
        Then(
            Map(PrimaryExpr, x => /* TODO: Wrap in sequence expr if needed */ x),
            Star(
                Or(
                    Map(Preceded(Whitespace, Predicate),
                        x => new Ast(AstNodeName.Predicate, x)),
                    Map(Preceded(Whitespace, ArgumentList),
                        x => new Ast(AstNodeName.ArgumentList, x))
                    // TODO: Preceded(Whitespace, Lookup()),
                )
            ),
            (expression, postfixExpr) =>
            {
                Either<Ast, Ast[]> toWrap = expression;

                var predicates = new List<Ast>();
                var filters = new List<Ast>();

                var allowSinglePredicates = false;

                var flushPredicates = (bool allowSinglePred) =>
                {
                    if (allowSinglePred && predicates.Count == 1)
                        filters.Add(new Ast(AstNodeName.Predicate) {Children = new List<Ast> {predicates[0]}});
                    else if (predicates.Count != 0)
                        filters.Add(new Ast(AstNodeName.Predicates) {Children = predicates});
                    predicates.Clear();
                };

                var flushFilters = (bool ensureFilter, bool allowSinglePred) =>
                {
                    flushPredicates(allowSinglePred);
                    if (filters.Count != 0)
                    {
                        if (toWrap.IsLeft() && toWrap.AsLeft().IsA(AstNodeName.SequenceExpr) &&
                            toWrap.AsLeft().Children.Count > 1)
                            toWrap = new Ast(AstNodeName.SequenceExpr, toWrap.AsLeft());

                        toWrap = new[] {new Ast(AstNodeName.FilterExpr, toWrap.AsLeft())}.Concat(filters).ToArray();
                        filters.Clear();
                    }
                    else if (ensureFilter)
                    {
                        toWrap = new[] {new Ast(AstNodeName.FilterExpr, toWrap.AsLeft())};
                    }
                    else
                    {
                        toWrap = new[] {toWrap.AsLeft()};
                    }
                };

                foreach (var postfix in postfixExpr)
                    switch (postfix.Name)
                    {
                        case AstNodeName.Predicate:
                            predicates.Add(postfix.GetFirstChild()!);
                            break;
                        case AstNodeName.Lookup:
                            allowSinglePredicates = true;
                            flushPredicates(allowSinglePredicates);
                            filters.Add(postfix);
                            break;
                        case AstNodeName.ArgumentList:
                            flushFilters(false, allowSinglePredicates);
                            if (toWrap.AsRight().Length > 1)
                                toWrap = new[]
                                {
                                    new Ast(AstNodeName.SequenceExpr,
                                        new Ast(AstNodeName.PathExpr, new Ast(AstNodeName.StepExpr, toWrap.AsRight())))
                                };

                            toWrap = new Ast(AstNodeName.DynamicFunctionInvocationExpr, new[]
                            {
                                new Ast(AstNodeName.FunctionItem, toWrap.AsRight())
                            }.Concat(postfix.Children.Count > 0
                                ? new[] {new Ast(AstNodeName.Arguments, postfix.Children.ToArray())}
                                : Array.Empty<Ast>()).ToArray());
                            break;
                        default:
                            throw new Exception("Unreachable");
                    }

                flushFilters(true, allowSinglePredicates);

                // TODO: technically this should be AsLeft but something is wrong with this current implementation
                return toWrap.AsRight()[0];
            }
        );

    private static readonly ParseFunc<Ast> StepExprWithForcedStep =
        Or(
            Map(PostfixExprWithStep, x => new Ast(AstNodeName.StepExpr, x)),
            AxisStep
        );

    private static readonly ParseFunc<Ast> PostfixExprWithoutStep =
        Followed(
            PrimaryExpr,
            Peek(
                // TODO: add lookup
                Not(Preceded(Whitespace, Or(Predicate, Map(ArgumentList, _ => new Ast(AstNodeName.All)))),
                    new[]
                    {
                        "Primary expression not followed by predicate, argumentList, or lookup"
                    })
            )
        );


    private static readonly ParseFunc<Ast> StepExprWithoutStep =
        PostfixExprWithoutStep;

    private static readonly ParseFunc<Ast> RelativePathExpr =
        Or(
            Then3(StepExprWithForcedStep,
                Preceded(Whitespace, LocationPathAbbreviation),
                Preceded(Whitespace, RelativePathExprWithForcedStepIndirect),
                (lhs, abbrev, rhs) => new Ast(AstNodeName.PathExpr, new[] {lhs, abbrev}.Concat(rhs).ToArray())),
            Then(
                StepExprWithForcedStep,
                Preceded(Surrounded(Token("/"), Whitespace), RelativePathExprWithForcedStepIndirect),
                (lhs, rhs) => new Ast(AstNodeName.PathExpr, new[] {lhs}.Concat(rhs).ToArray())),
            StepExprWithoutStep,
            Map(
                StepExprWithForcedStep, x =>
                    new Ast(AstNodeName.PathExpr, x)
            )
        );

    private static readonly ParseFunc<Ast[]> RelativePathExprWithForcedStep =
        Or(
            Then3(
                StepExprWithForcedStep,
                Preceded(Whitespace, LocationPathAbbreviation),
                Preceded(Whitespace, RelativePathExprWithForcedStepIndirect),
                (lhs, abbrev, rhs) => new[] {lhs, abbrev}.Concat(rhs).ToArray()
            ),
            Then(
                StepExprWithForcedStep,
                Preceded(Surrounded(Token("/"), Whitespace), RelativePathExprWithForcedStepIndirect),
                (lhs, rhs) => new[] {lhs}.Concat(rhs).ToArray()), Map(StepExprWithForcedStep, x => new[] {x}),
            Map(StepExprWithForcedStep, x => new[] {x})
        );

    private static ParseResult<Ast[]> RelativePathExprWithForcedStepIndirect(string input, int offset)
    {
        return RelativePathExprWithForcedStep(input, offset);
    }

    // TODO: add other variants
    private static readonly ParseFunc<Ast> PathExpr =
        Or(RelativePathExpr);

    private static readonly ParseFunc<Ast> ValueExpr =
        Or(
            // TODO: ValidateExpr(),
            // TODO: ExtensionExpr(),
            // TODO: SimpleMapExpr(),
            PathExpr
        );

    private static readonly ParseFunc<Ast> UnaryExpr =
        Or(
            Then(
                Or(
                    Alias(AstNodeName.UnaryMinusOp, "-"),
                    Alias(AstNodeName.UnaryPlusOp, "+")
                ),
                Preceded(Whitespace, UnaryExprIndirect),
                (op, value) => new Ast(op, new Ast(AstNodeName.Operand, value))
            ),
            ValueExpr
        );

    private static readonly ParseFunc<Ast> ArrowFunctionSpecifier =
        Or(Map(EqName, x => x.GetAst(AstNodeName.EqName))
            // TODO: VarRef(),
            // TODO: ParenthesizedExpr(),
        );

    private static readonly ParseFunc<Ast> ArrowExpr =
        Then(
            UnaryExpr,
            Star(
                PrecededMultiple(
                    new[]
                    {
                        Whitespace, Token("=>"), Whitespace
                    },
                    Then(
                        ArrowFunctionSpecifier,
                        Preceded(Whitespace, ArgumentList),
                        (specifier, argList) => (specifier, argList)
                    )
                )
            ),
            (Ast argExpr, (Ast, Ast[])[] functionParts) => functionParts.Aggregate(argExpr,
                (arg, part) => new Ast(AstNodeName.ArrowExpr, new Ast(AstNodeName.ArgExpr, arg), part.Item1,
                    new Ast(AstNodeName.Arguments, part.Item2)))
        );

    private static readonly ParseFunc<Ast> CastExpr =
        Then(
            ArrowExpr,
            Optional(
                PrecededMultiple(
                    new[]
                    {
                        Whitespace,
                        Token("cast"),
                        WhitespacePlus,
                        Token("as"),
                        AssertAdjacentOpeningTerminal,
                        Whitespace
                    },
                    SingleType)
            ),
            (lhs, rhs) =>
                rhs != null
                    ? new Ast(AstNodeName.CastExpr, new Ast(AstNodeName.ArgExpr, lhs),
                        rhs)
                    : lhs
        );

    private static readonly ParseFunc<Ast> CastableExpr =
        Then(
            CastExpr,
            Optional(
                PrecededMultiple(
                    new[]
                    {
                        Whitespace,
                        Token("castable"),
                        WhitespacePlus,
                        Token("as"),
                        AssertAdjacentOpeningTerminal,
                        Whitespace
                    },
                    SingleType)
            ),
            (lhs, rhs) =>
                rhs != null
                    ? new Ast(AstNodeName.CastableExpr, new Ast(AstNodeName.ArgExpr, lhs),
                        rhs)
                    : lhs
        );

    private static readonly ParseFunc<Ast> TreatExpr = Then(
        CastableExpr,
        Optional(
            PrecededMultiple(
                new[]
                {
                    Whitespace,
                    Token("treat"),
                    WhitespacePlus,
                    Token("as"),
                    AssertAdjacentOpeningTerminal,
                    Whitespace
                },
                SequenceType)
        ),
        (lhs, rhs) =>
            rhs != null
                ? new Ast(AstNodeName.TreatExpr, new Ast(AstNodeName.ArgExpr, lhs),
                    new Ast(AstNodeName.SequenceType, rhs))
                : lhs
    );

    private static readonly ParseFunc<Ast> InstanceOfExpr =
        Then(
            TreatExpr,
            Optional(
                PrecededMultiple(new[]
                    {
                        Whitespace,
                        Token("instance"),
                        WhitespacePlus,
                        Token("of"),
                        AssertAdjacentOpeningTerminal,
                        Whitespace
                    },
                    SequenceType)
            ),
            (lhs, rhs) =>
                rhs != null
                    ? new Ast(AstNodeName.InstanceOfExpr, new Ast(AstNodeName.ArgExpr, lhs),
                        new Ast(AstNodeName.SequenceType, rhs))
                    : lhs
        );

    private static readonly ParseFunc<Ast> IntersectExpr =
        BinaryOperator(
            InstanceOfExpr,
            Followed(
                Or(
                    Alias(AstNodeName.IntersectOp, "intersectOp"),
                    Alias(AstNodeName.ExceptOp, "exceptOp")
                ),
                AssertAdjacentOpeningTerminal
            ),
            DefaultBinaryOperatorFn
        );

    private static readonly ParseFunc<Ast> UnionExpr =
        BinaryOperator(IntersectExpr,
            Or(
                Alias(AstNodeName.UnionOp, "|"),
                Followed(Alias(AstNodeName.UnionOp, "union"), AssertAdjacentOpeningTerminal)
            ),
            DefaultBinaryOperatorFn
        );

    private static readonly ParseFunc<Ast> MultiplicativeExpr =
        BinaryOperator(
            UnionExpr,
            Or(
                Alias(AstNodeName.MultiplyOp, "*"),
                Followed(Alias(AstNodeName.DivOp, "div"), AssertAdjacentOpeningTerminal),
                Followed(Alias(AstNodeName.IdivOp, "idiv"), AssertAdjacentOpeningTerminal),
                Followed(Alias(AstNodeName.ModOp, "mod"), AssertAdjacentOpeningTerminal)
            ),
            DefaultBinaryOperatorFn
        );

    private static readonly ParseFunc<Ast> AdditiveExpr =
        BinaryOperator(MultiplicativeExpr, Or(
            Alias(AstNodeName.SubtractOp, "-"),
            Alias(AstNodeName.AddOp, "+")
        ), DefaultBinaryOperatorFn);

    private static readonly ParseFunc<Ast> RangeExpr =
        NonRepeatableBinaryOperator(AdditiveExpr,
            Followed(Alias(AstNodeName.RangeSequenceExpr, "to"), AssertAdjacentOpeningTerminal));

    private static readonly ParseFunc<Ast> StringConcatExpr =
        BinaryOperator(RangeExpr, Alias(AstNodeName.StringConcatenateOp, "||"), DefaultBinaryOperatorFn);

    private static readonly ParseFunc<Ast> ComparisonExpr =
        NonRepeatableBinaryOperator(StringConcatExpr, Or(
            // TODO: ValueCompare(),
            // TODO: NodeCompare(),
            // TODO: GeneralCompare()
            Alias(AstNodeName.All, "AAAAAAAAAAAAAAAA")
        ));

    private static readonly ParseFunc<Ast> AndExpr =
        BinaryOperator(ComparisonExpr,
            Followed(Alias(AstNodeName.AndOp, "and"), AssertAdjacentOpeningTerminal),
            DefaultBinaryOperatorFn);

    private static readonly ParseFunc<Ast> OrExpr =
        BinaryOperator(AndExpr,
            Followed(Alias(AstNodeName.OrOp, "or"), AssertAdjacentOpeningTerminal),
            DefaultBinaryOperatorFn);

    public static readonly ParseFunc<Ast> QueryBody =
        Map(Expr(), x => new Ast(AstNodeName.QueryBody, x));

    private static Ast DefaultBinaryOperatorFn(Ast lhs, IEnumerable<(AstNodeName, Ast)> rhs)
    {
        return rhs.Aggregate(lhs, (lh, rh) =>
            new Ast(rh.Item1, new Ast(AstNodeName.FirstOperand, lh), new Ast(AstNodeName.SecondOperand, rh.Item2)));
    }

    private static ParseFunc<TS> BinaryOperator<T, TS>(ParseFunc<T> expr,
        ParseFunc<AstNodeName> op,
        Func<T, (AstNodeName, T)[], TS> constructionFn)
    {
        return Then(
            expr,
            Star(Then(Surrounded(op, Whitespace), expr, (a, b) => (a, b))),
            constructionFn
        );
    }

    private static ParseFunc<Ast> NonRepeatableBinaryOperator(ParseFunc<Ast> expr,
        ParseFunc<AstNodeName> op,
        AstNodeName firstArgName = AstNodeName.FirstOperand,
        AstNodeName secondArgName = AstNodeName.SecondOperand)
    {
        return Then(
            expr,
            OptionalDefaultValue(Then(
                Surrounded(op, Whitespace),
                expr,
                (a, b) => (a, b)
            )),
            (lhs, rhs) =>
            {
                if (rhs == null) return lhs;

                return new Ast(rhs.Value.Item1, new Ast(firstArgName, lhs),
                    new Ast(secondArgName, rhs.Value.Item2));
            }
        );
    }

    private static ParseResult<Ast> UnaryExprIndirect(string input, int offset)
    {
        return UnaryExpr(input, offset);
    }

    private static ParseResult<Ast> ExprSingle(string input, int offset)
    {
        // TODO: wrap in stacktrace
        return Or(OrExpr)(input, offset);
    }

    private static ParseFunc<Ast> Expr()
    {
        return BinaryOperator(ExprSingle, Alias(AstNodeName.SequenceExpr, ","), (lhs, rhs) =>
            rhs.Length == 0
                ? lhs
                : new Ast(AstNodeName.SequenceExpr, rhs.Select(x => x.Item2).ToArray()));
    }
}