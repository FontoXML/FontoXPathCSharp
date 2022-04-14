using FontoXPathCSharp.Value;
using PrscSharp;
using static PrscSharp.PrscSharp;
using static FontoXPathCSharp.Parsing.NameParser;
using static FontoXPathCSharp.Parsing.WhitespaceParser;
using static FontoXPathCSharp.Parsing.LiteralParser;

namespace FontoXPathCSharp.Parsing;

public static class XPathParser
{
    private static ParseFunc<ParseResult<TS>> BinaryOperator<T, TS>(ParseFunc<ParseResult<T>> expr,
        ParseFunc<ParseResult<AstNodeName>> op,
        Func<T, (AstNodeName, T)[], TS> constructionFn)
    {
        return Then(
            expr,
            Star(Then(Surrounded(op, Whitespace()), expr, (a, b) => (a, b))),
            constructionFn
        );
    }

    private static Ast DefaultBinaryOperatorFn(Ast lhs, IEnumerable<(AstNodeName, Ast)> rhs)
    {
        return rhs.Aggregate(lhs, (lh, rh) =>
            new Ast(rh.Item1, new Ast(AstNodeName.FirstOperand, lh), new Ast(AstNodeName.SecondOperand, rh.Item2)));
    }

    private static ParseFunc<ParseResult<Ast>> NonRepeatableBinaryOperator(ParseFunc<ParseResult<Ast>> expr,
        ParseFunc<ParseResult<AstNodeName>> op,
        AstNodeName firstArgName = AstNodeName.FirstOperand,
        AstNodeName secondArgName = AstNodeName.SecondOperand)
    {
        return Then(
            expr,
            OptionalDefaultValue(Then(
                Surrounded(op, Whitespace()),
                expr,
                (a, b) => (a, b)
            )),
            (lhs, rhs) =>
            {
                if (rhs == null)
                {
                    return lhs;
                }

                return new Ast(rhs.Value.Item1, new Ast(firstArgName, lhs),
                    new Ast(secondArgName, rhs.Value.Item2));
            }
        );
    }

    private static ParseFunc<ParseResult<T>> PrecededMultiple<TBefore, T>(
        IEnumerable<ParseFunc<ParseResult<TBefore>>> before,
        ParseFunc<ParseResult<T>> parser)
    {
        return before.Aggregate(parser, (current, b) => Preceded(b, current));
    }


    private static ParseFunc<ParseResult<TR>> Then3<T1, T2, T3, TR>(ParseFunc<ParseResult<T1>> parser1,
        ParseFunc<ParseResult<T2>> parser2, ParseFunc<ParseResult<T3>> parser3,
        Func<T1, T2, T3, TR> join)
    {
        return Then(parser1, Then(parser2, parser3, (b, c) => (b, c)), (a, bc) => join(a, bc.b, bc.c));
    }

    private static ParseFunc<ParseResult<T>> Alias<T>(T aliasedValue, params string[] tokenNames)
    {
        return Map(Or(tokenNames.Select(Token).ToArray()), _ => aliasedValue);
    }

    private static ParseFunc<ParseResult<string>> AssertAdjacentOpeningTerminal()
    {
        return Peek(Or(
            Token("("), Token("\""), Token("'"), WhitespaceCharacter()));
    }

    private static readonly ParseFunc<ParseResult<string>> ForwardAxis =
        Map(Or(
            Token("self::")
            // TODO: add other variants
        ), x => x[..^2]);


    private static readonly ParseFunc<ParseResult<QName>> UnprefixedName =
        Map(NcName(), x => new QName(x, null, ""));


    private static readonly ParseFunc<ParseResult<QName>> QName =
        Or(
            UnprefixedName
            // TODO: add prefixed name
        );


    // TODO: add uriQualifiedName
    private static readonly ParseFunc<ParseResult<QName>> EqName = Or(QName);

    // TODO: add wildcard
    private static readonly ParseFunc<ParseResult<Ast>> NameTest =
        Map(EqName, x =>
            new Ast(AstNodeName.NameTest)
            {
                StringAttributes =
                {
                    ["URI"] = x.NamespaceUri!,
                    ["prefix"] = x.Prefix
                },
                TextContent = x.LocalName
            });

    // TODO: add kindTest
    private static readonly ParseFunc<ParseResult<Ast>> NodeTest = Or(NameTest);

    private static readonly ParseFunc<ParseResult<Ast>> ForwardStep
        = Or(Then(ForwardAxis, NodeTest,
            (axis, test) =>
                new Ast(AstNodeName.StepExpr,
                    new Ast(AstNodeName.XPathAxis) {TextContent = axis},
                    test
                )));

    // TODO: add predicateList
    // TODO: add reverse step
    private static readonly ParseFunc<ParseResult<Ast>> AxisStep =
        Or(ForwardStep);

    // TODO: add string literal
    private static readonly ParseFunc<ParseResult<Ast>> Literal =
        Or(NumericLiteral());

    private static ParseFunc<ParseResult<Ast>> PrimaryExpr()
    {
        // TODO: add others
        return
            Or(Literal, FunctionCall);
    }

    
    // TODO: add argumentPlaceholder
    private static readonly ParseFunc<ParseResult<Ast>> Argument =
        ExprSingle;

    private static readonly ParseFunc<ParseResult<Ast[]>> ArgumentList =
        Map(
            Delimited(
                Token("("),
                Surrounded(
                    Optional(
                        Then(Argument,
                            Star(Preceded(Surrounded(Token(","), Whitespace()), Argument)),
                            (first, following) => following.Prepend(first).ToArray())
                    ),
                    Whitespace()
                ),
                Token(")")
            ),
            x => x ?? Array.Empty<Ast>()
        );

    private static readonly ParseFunc<ParseResult<Ast>> Predicate =
        Delimited(Token("["), Surrounded(Expr(), Whitespace()), Token("]"));

    private static readonly ParseFunc<ParseResult<Ast>> PostfixExprWithStep =
        Then(
            Map(PrimaryExpr(), x => /* TODO: Wrap in sequence expr if needed */ x),
            Star(
                Or(
                    Map(Preceded(Whitespace(), Predicate),
                        x => new Ast(AstNodeName.Predicate, x)),
                    Map(Preceded(Whitespace(), ArgumentList),
                        x => new Ast(AstNodeName.ArgumentList, x))
                    // TODO: Preceded(Whitespace(), Lookup()),
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
                        if (toWrap.AsLeft() == AstNodeName.SequenceExpr && toWrap.AsLeft().Children.Count > 1)
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
                            if (toWrap.AsRight().Length == 1)
                                toWrap = new[]
                                {
                                    new Ast(AstNodeName.SequenceExpr,
                                        new Ast(AstNodeName.PathExpr, new Ast(AstNodeName.StepExpr, toWrap.AsRight())))
                                };
                            toWrap = new Ast(AstNodeName.DynamicFunctionInvocationExpr, new[]
                            {
                                new Ast(AstNodeName.FunctionItem, toWrap.AsRight())
                            }.Concat(postfix.Children.Count != 0
                                ? new[] {new Ast(AstNodeName.Arguments, postfix.Children[0])}
                                : Array.Empty<Ast>()).ToArray());
                            break;
                        default:
                            throw new Exception("Unreachable");
                    }

                flushFilters(true, allowSinglePredicates);

                return toWrap.AsLeft();
            }
        );

    private static readonly ParseFunc<ParseResult<Ast>> StepExprWithForcedStep =
        Or(
            Map(PostfixExprWithStep, x => new Ast(AstNodeName.StepExpr, x)),
            AxisStep
        );


    private static readonly ParseFunc<ParseResult<Ast>> PostfixExprWithoutStep =
        Followed(
            PrimaryExpr(),
            Peek(
                // TODO: add lookup
                Not(Preceded(Whitespace(), Or(Predicate, Map(ArgumentList, _ => new Ast(AstNodeName.All)))),
                    new[]
                    {
                        "Primary expression not followed by predicate, argumentList, or lookup"
                    })
            )
        );


    private static readonly ParseFunc<ParseResult<Ast>> StepExprWithoutStep =
        PostfixExprWithoutStep;


    private static readonly ParseFunc<ParseResult<Ast>> LocationPathAbbreviation =
        Map(Token("//"), _ =>

            // TODO: convert descendant-or-self to enum
            new Ast(AstNodeName.StepExpr, new Ast(AstNodeName.XPathAxis)
                {
                    TextContent = "descendant-or-self"
                },
                new Ast(AstNodeName.AnyKindTest))
        );

    private static readonly ParseFunc<ParseResult<Ast[]>> RelativePathExprWithForcedStep =
        Or(
            Then3(
                StepExprWithForcedStep,
                Preceded(Whitespace(), LocationPathAbbreviation),
                Preceded(Whitespace(), RelativePathExprWithForcedStepIndirect),
                (lhs, abbrev, rhs) => new[] {lhs, abbrev}.Concat(rhs).ToArray()
            ),
            Then(
                StepExprWithForcedStep,
                Preceded(Surrounded(Token("/"), Whitespace()), RelativePathExprWithForcedStepIndirect),
                (lhs, rhs) => new[] {lhs}.Concat(rhs).ToArray()), Map(StepExprWithForcedStep, x => new[] {x}),
            Map(StepExprWithForcedStep, x => new[] {x})
        );


    private static ParseResult<Ast[]> RelativePathExprWithForcedStepIndirect(string input, int offset)
    {
        return RelativePathExprWithForcedStep(input, offset);
    }

    private static readonly ParseFunc<ParseResult<Ast>> RelativePathExpr =
        Or(
            Then3(StepExprWithForcedStep,
                Preceded(Whitespace(), LocationPathAbbreviation),
                Preceded(Whitespace(), RelativePathExprWithForcedStep),
                (lhs, abbrev, rhs) => new Ast(AstNodeName.PathExpr, new[] {lhs, abbrev}.Concat(rhs).ToArray())),
            Then(
                StepExprWithForcedStep,
                Preceded(Surrounded(Token("/"), Whitespace()), RelativePathExprWithForcedStep),
                (lhs, rhs) => new Ast(AstNodeName.PathExpr, new[] {lhs}.Concat(rhs).ToArray())),
            StepExprWithoutStep,
            Map(
                StepExprWithForcedStep, x =>
                    new Ast(AstNodeName.PathExpr, x)
            )
        );

    // TODO: add other variants
    public static readonly ParseFunc<ParseResult<Ast>> PathExpr =
        Or(RelativePathExpr);

    private static readonly ParseFunc<ParseResult<string>> ReservedFunctionNames =
        Or(new[]
        {
            "array",
            "attribute",
            "comment",
            "document-node",
            "element",
            "empty-sequence",
            "function",
            "if",
            "item",
            "map",
            "namespace-node",
            "node",
            "processing-instruction",
            "schema-attribute",
            "schema-element",
            "switch",
            "text",
            "typeswitch"
        }.Select(Token).ToArray());

    private static readonly ParseFunc<ParseResult<Ast>> FunctionCall =
        Preceded(
            Not(Followed(ReservedFunctionNames, new[] {Whitespace(), Token("(")}),
                new[] {"cannot use reserved keyword for function names"}),
            Then(EqName, Preceded(Whitespace(), ArgumentList),
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


    private static ParseFunc<ParseResult<Ast>> Expr()
    {
        return BinaryOperator(ExprSingle, Alias(AstNodeName.SequenceExpr, ","), (lhs, rhs) =>
            rhs.Length == 0
                ? lhs
                : new Ast(AstNodeName.SequenceExpr, rhs.Select(x => x.Item2).ToArray()));
    }

    private static readonly ParseFunc<ParseResult<Ast>> ValueExpr =
        Or(
            // TODO: ValidateExpr(),
            // TODO: ExtensionExpr(),
            // TODO: SimpleMapExpr(),
            PathExpr
        );

    private static readonly ParseFunc<ParseResult<Ast>> UnaryExpr =
        Or(
            Then(
                Or(
                    Alias(AstNodeName.UnaryMinusOp, "-"),
                    Alias(AstNodeName.UnaryPlusOp, "+")
                ),
                Preceded(Whitespace(), UnaryExprIndirect),
                (op, value) => new Ast(op, new Ast(AstNodeName.Operand, value))
            ),
            ValueExpr
        );

    private static ParseResult<Ast> UnaryExprIndirect(string input, int offset)
    {
        return UnaryExpr(input, offset);
    }

    private static readonly ParseFunc<ParseResult<Ast>> ArrowFunctionSpecifier =
        Or(Map(EqName, x => x.GetAst(AstNodeName.EqName))
            // TODO: VarRef(),
            // TODO: ParenthesizedExpr(),
        );

    private static readonly ParseFunc<ParseResult<Ast>> ArrowExpr =
        Then(
            UnaryExpr,
            Star(
                PrecededMultiple(
                    new[]
                    {
                        Whitespace(), Token("=>"), Whitespace()
                    },
                    Then(
                        ArrowFunctionSpecifier,
                        Preceded(Whitespace(), ArgumentList),
                        (specifier, argList) => (specifier, argList)
                    )
                )
            ),
            (Ast argExpr, (Ast, Ast[])[] functionParts) => functionParts.Aggregate(argExpr,
                (arg, part) => new Ast(AstNodeName.ArrowExpr, new Ast(AstNodeName.ArgExpr, arg), part.Item1,
                    new Ast(AstNodeName.Arguments, part.Item2)))
        );

    private static readonly ParseFunc<ParseResult<Ast>> CastExpr =
        Then(
            ArrowExpr,
            Optional(
                PrecededMultiple(
                    new[]
                    {
                        Whitespace(),
                        Token("cast"),
                        WhitespacePlus(),
                        Token("as"),
                        AssertAdjacentOpeningTerminal(),
                        Whitespace()
                    },
                    SingleType())
            ),
            (lhs, rhs) =>
                rhs != null
                    ? new Ast(AstNodeName.CastExpr, new Ast(AstNodeName.ArgExpr, lhs),
                        rhs)
                    : lhs
        );

    private static readonly ParseFunc<ParseResult<Ast>> CastableExpr =
        Then(
            CastExpr,
            Optional(
                PrecededMultiple(
                    new[]
                    {
                        Whitespace(),
                        Token("castable"),
                        WhitespacePlus(),
                        Token("as"),
                        AssertAdjacentOpeningTerminal(),
                        Whitespace()
                    },
                    SingleType())
            ),
            (lhs, rhs) =>
                rhs != null
                    ? new Ast(AstNodeName.CastableExpr, new Ast(AstNodeName.ArgExpr, lhs),
                        rhs)
                    : lhs
        );

    private static readonly ParseFunc<ParseResult<Ast>> TreatExpr = Then(
        CastableExpr,
        Optional(
            PrecededMultiple(
                new[]
                {
                    Whitespace(),
                    Token("treat"),
                    WhitespacePlus(),
                    Token("as"),
                    AssertAdjacentOpeningTerminal(),
                    Whitespace()
                },
                SequenceType())
        ),
        (lhs, rhs) =>
            rhs != null
                ? new Ast(AstNodeName.TreatExpr, new Ast(AstNodeName.ArgExpr, lhs),
                    new Ast(AstNodeName.SequenceType, rhs))
                : lhs
    );

    private static ParseFunc<ParseResult<Ast>> SingleType()
    {
        return Map(Token("AAAAAAAAAAA"), _ => new Ast(AstNodeName.All));
    }

    private static ParseFunc<ParseResult<Ast>> SequenceType()
    {
        return Map(Token("AAAAAAAAAAA"), _ => new Ast(AstNodeName.All));
    }

    private static readonly ParseFunc<ParseResult<Ast>> InstanceOfExpr =
        Then(
            TreatExpr,
            Optional(
                PrecededMultiple(new[]
                    {
                        Whitespace(),
                        Token("instance"),
                        WhitespacePlus(),
                        Token("of"),
                        AssertAdjacentOpeningTerminal(),
                        Whitespace(),
                    },
                    SequenceType())
            ),
            (lhs, rhs) =>
                rhs != null
                    ? new Ast(AstNodeName.InstanceOfExpr, new Ast(AstNodeName.ArgExpr, lhs),
                        new Ast(AstNodeName.SequenceType, rhs))
                    : lhs
        );

    private static readonly ParseFunc<ParseResult<Ast>> IntersectExpr =
        BinaryOperator(
            InstanceOfExpr,
            Followed(
                Or(
                    Alias(AstNodeName.IntersectOp, "intersectOp"),
                    Alias(AstNodeName.ExceptOp, "exceptOp")
                ),
                AssertAdjacentOpeningTerminal()
            ),
            DefaultBinaryOperatorFn
        );

    private static readonly ParseFunc<ParseResult<Ast>> UnionExpr =
        BinaryOperator(IntersectExpr,
            Or(
                Alias(AstNodeName.UnionOp, "|"),
                Followed(Alias(AstNodeName.UnionOp, "union"), AssertAdjacentOpeningTerminal())
            ),
            DefaultBinaryOperatorFn
        );

    private static readonly ParseFunc<ParseResult<Ast>> MultiplicativeExpr =
        BinaryOperator(
            UnionExpr,
            Or(
                Alias(AstNodeName.MultiplyOp, "*"),
                Followed(Alias(AstNodeName.DivOp, "div"), AssertAdjacentOpeningTerminal()),
                Followed(Alias(AstNodeName.IdivOp, "idiv"), AssertAdjacentOpeningTerminal()),
                Followed(Alias(AstNodeName.ModOp, "mod"), AssertAdjacentOpeningTerminal())
            ),
            DefaultBinaryOperatorFn
        );

    private static readonly ParseFunc<ParseResult<Ast>> AdditiveExpr =
        BinaryOperator(MultiplicativeExpr, Or(
            Alias(AstNodeName.SubtractOp, "-"),
            Alias(AstNodeName.AddOp, "+")
        ), DefaultBinaryOperatorFn);

    private static readonly ParseFunc<ParseResult<Ast>> RangeExpr =
        NonRepeatableBinaryOperator(AdditiveExpr,
            Followed(Alias(AstNodeName.RangeSequenceExpr, "to"), AssertAdjacentOpeningTerminal()));

    private static readonly ParseFunc<ParseResult<Ast>> StringConcatExpr =
        BinaryOperator(RangeExpr, Alias(AstNodeName.StringConcatenateOp, "||"), DefaultBinaryOperatorFn);

    private static readonly ParseFunc<ParseResult<Ast>> ComparisonExpr =
        NonRepeatableBinaryOperator(StringConcatExpr, Or(
            // TODO: ValueCompare(),
            // TODO: NodeCompare(),
            // TODO: GeneralCompare()
            Alias(AstNodeName.All, "AAAAAAAAAAAAAAAA")
        ));

    private static readonly ParseFunc<ParseResult<Ast>> AndExpr =
        BinaryOperator(ComparisonExpr,
            Followed(Alias(AstNodeName.AndOp, "and"), AssertAdjacentOpeningTerminal()),
            DefaultBinaryOperatorFn);

    private static readonly ParseFunc<ParseResult<Ast>> OrExpr =
        BinaryOperator(AndExpr,
            Followed(Alias(AstNodeName.OrOp, "or"), AssertAdjacentOpeningTerminal()),
            DefaultBinaryOperatorFn);


    private static ParseResult<Ast> ExprSingle(string input, int offset)
    {
        // TODO: wrap in stacktrace
        return Or(OrExpr)(input, offset);
    }

    public static readonly ParseFunc<ParseResult<Ast>> QueryBody =
        Map(Expr(), x => new Ast(AstNodeName.QueryBody, x));
}