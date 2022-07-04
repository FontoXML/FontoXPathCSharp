using FontoXPathCSharp.Value;
using PrscSharp;
using static PrscSharp.PrscSharp;
using static FontoXPathCSharp.Parsing.NameParser;
using static FontoXPathCSharp.Parsing.ParsingFunctions;
using static FontoXPathCSharp.Parsing.WhitespaceParser;
using static FontoXPathCSharp.Parsing.LiteralParser;
using static FontoXPathCSharp.Parsing.TypesParser;

namespace FontoXPathCSharp.Parsing;

public readonly struct ParseOptions
{
    public bool OutputDebugInfo { get; }
    public bool XQuery { get; }

    public ParseOptions(bool outputDebugInfo, bool xquery)
    {
        OutputDebugInfo = outputDebugInfo;
        XQuery = xquery;
    }
}

public static class XPathParser
{
    private static ParseOptions _options;

    private static readonly ParseFunc<Ast> Predicate =
        Delimited(Token("["), Surrounded(Expr(), Whitespace), Token("]"));

    private static readonly ParseFunc<string> StringLiteral = Map(_options.XQuery
            ? Or(Surrounded(Star(Or(PredefinedEntityRef, CharRef, EscapeQuot, Regex("[^\"&]"))), Token("\"")),
                Surrounded(Star(Or(PredefinedEntityRef, CharRef, EscapeApos, Regex("[^'&]"))), Token("'")))
            : Or(Surrounded(Star(Or(EscapeQuot, Regex("[^\"]"))), Token("\"")),
                Surrounded(Star(Or(EscapeApos, Regex("[^']"))), Token("'"))),
        x => string.Join("", x)
    );

    private static readonly ParseFunc<Ast> ElementTest = Or(
        Map(
            PrecededMultiple(new[] {Token("element"), Whitespace},
                Delimited(
                    Followed(Token("("), Whitespace),
                    Then(
                        ElementNameOrWildcard,
                        PrecededMultiple(new[] {Whitespace, Token(","), Whitespace}, TypeName),
                        (elemName, typeName) => (
                            nameOrWildcard: new Ast(AstNodeName.ElementName, elemName),
                            type: typeName.GetAst(AstNodeName.TypeName)
                        )
                    ),
                    Preceded(Whitespace, Token(")"))
                )
            ),
            x => new Ast(AstNodeName.ElementTest, x.nameOrWildcard, x.type)
        ),
        Map(
            PrecededMultiple(new[] {Token("element"), Whitespace},
                Delimited(
                    Token("("), ElementNameOrWildcard, Token(")")
                )
            ),
            nameOrWildcard => new Ast(AstNodeName.ElementTest, new Ast(AstNodeName.ElementName, nameOrWildcard))
        ),
        Map(
            PrecededMultiple(new[] {Token("element"), Whitespace},
                Delimited(
                    Token("("), Whitespace, Token(")")
                )
            ),
            nameOrWildcard => new Ast(AstNodeName.ElementTest)
        )
    );

    private static readonly ParseFunc<Ast> AttributeTest = Or(
        Map(
            PrecededMultiple(new[] {Token("attribute"), Whitespace},
                Delimited(
                    Followed(Token("("), Whitespace),
                    Then(
                        AttributeNameOrWildcard,
                        PrecededMultiple(new[] {Whitespace, Token(","), Whitespace}, TypeName),
                        (attrName, typeName) => (
                            nameOrWildcard: new Ast(AstNodeName.AttributeName, attrName),
                            type: typeName.GetAst(AstNodeName.TypeName)
                        )
                    ),
                    Preceded(Whitespace, Token(")"))
                )
            ),
            x => new Ast(AstNodeName.AttributeTest, x.nameOrWildcard, x.type)
        ),
        Map(
            PrecededMultiple(new[] {Token("attribute"), Whitespace},
                Delimited(
                    Token("("), AttributeNameOrWildcard, Token(")")
                )
            ),
            nameOrWildcard => new Ast(AstNodeName.AttributeTest, new Ast(AstNodeName.AttributeName, nameOrWildcard))
        ),
        Map(
            PrecededMultiple(new[] {Token("attribute"), Whitespace},
                Delimited(
                    Token("("), Whitespace, Token(")")
                )
            ),
            nameOrWildcard => new Ast(AstNodeName.AttributeTest)
        )
    );

    private static readonly ParseFunc<QName> ElementDeclaration = ElementName;

    private static readonly ParseFunc<Ast> SchemaElementTest = Map(
        Delimited(
            Token("schema-element("),
            Surrounded(ElementDeclaration, Whitespace),
            Token(")")
        ),
        x => x.GetAst(AstNodeName.SchemaElementTest)
    );

    private static readonly ParseFunc<QName> AttributeName = EqName;

    private static readonly ParseFunc<QName> AttributeDeclaration = AttributeName;

    private static readonly ParseFunc<Ast> SchemaAttributeTest = Map(
        Delimited(
            Token("schema-attribute("),
            Surrounded(AttributeDeclaration, Whitespace),
            Token(")")
        ),
        decl => decl.GetAst(AstNodeName.SchemaAttributeTest)
    );

    private static readonly ParseFunc<Ast> PiTest = Or(
        Map(
            Delimited(
                Token("processing-instruction("),
                Surrounded(Or(NcName, StringLiteral), Whitespace),
                Token(")")
            ),
            target => new Ast(AstNodeName.PiTest, new Ast(AstNodeName.PiTarget)
            {
                TextContent = target
            })
        ),
        Alias(new Ast(AstNodeName.PiTest), "processing-instruction()")
    );

    private static readonly ParseFunc<Ast> DocumentTest = Map(
        Delimited(Token("document-node("),
            Surrounded(Optional(Or(ElementTest, SchemaElementTest)), Whitespace),
            Token(")")
        ),
        x => x == null ? new Ast(AstNodeName.DocumentTest) : new Ast(AstNodeName.DocumentTest, x)
    );

    private static readonly ParseFunc<Ast> KindTest = Or(
        DocumentTest,
        ElementTest,
        AttributeTest,
        SchemaElementTest,
        SchemaAttributeTest,
        PiTest,
        CommentTest,
        TextTest,
        NamespaceNodeTest,
        AnyKindTest
    );

    private static readonly ParseFunc<Ast> Wildcard = Or(
        Map(Preceded(Token("*:"), NcName),
            x => new Ast(AstNodeName.Wildcard, new Ast(AstNodeName.Star),
                new Ast(AstNodeName.NcName) {TextContent = x})
        ),
        Alias(new Ast(AstNodeName.Wildcard), "*"),
        Map(Followed(BracedUriLiteral, Token("*")),
            x => new Ast(AstNodeName.Wildcard, new Ast(AstNodeName.Uri) {TextContent = x},
                new Ast(AstNodeName.Star))),
        Map(Followed(NcName, Token(":*")), x =>
            new Ast(AstNodeName.Wildcard, new Ast(AstNodeName.NcName) {TextContent = x}, new Ast(AstNodeName.Star))
        )
    );

    private static readonly ParseFunc<Ast> NameTest = Or(
        Wildcard,
        Map(EqName, x => new Ast(AstNodeName.NameTest)
            {
                StringAttributes =
                {
                    ["URI"] = x.NamespaceUri!,
                    ["prefix"] = x.Prefix
                },
                TextContent = x.LocalName
            }
        )
    );

    private static readonly ParseFunc<Ast> NodeTest = Or(KindTest, NameTest);

    private static readonly ParseFunc<Ast> AbbrevForwardStep = Then(Optional(Token("@")), NodeTest,
        (a, b) => new Ast(AstNodeName.StepExpr, new Ast(AstNodeName.XPathAxis, b)
        {
            TextContent = a != null || b.IsA(AstNodeName.AttributeTest, AstNodeName.SchemaAttributeTest)
                ? "attribute"
                : "child"
        }, b));

    private static readonly ParseFunc<Ast> ForwardStep
        = Or(Then(ForwardAxis, NodeTest,
                (axis, test) =>
                    new Ast(AstNodeName.StepExpr,
                        new Ast(AstNodeName.XPathAxis) {TextContent = axis},
                        test
                    )),
            AbbrevForwardStep);

    private static readonly ParseFunc<Ast> AbbrevReverseStep = Map(Token(".."), _ =>
        new Ast(AstNodeName.StepExpr, new Ast(AstNodeName.XPathAxis) {TextContent = "parent"},
            new Ast(AstNodeName.AnyKindTest))
    );

    private static readonly ParseFunc<Ast> ReverseStep
        = Or(
            Then(ReverseAxis, NodeTest,
                (axis, test) =>
                    new Ast(AstNodeName.StepExpr,
                        new Ast(AstNodeName.XPathAxis) {TextContent = axis},
                        test
                    )),
            AbbrevReverseStep);

    private static readonly ParseFunc<Ast?> PredicateList = Map(Star(Preceded(Whitespace, Predicate)),
        x => x.Length > 0 ? new Ast(AstNodeName.Predicates, x) : null);

    private static readonly ParseFunc<Ast> AxisStep =
        Then(
            Or(ReverseStep, ForwardStep),
            PredicateList,
            (a, b) =>
            {
                if (b == null) return a;
                a.Children.Add(b);
                return a;
            });

    private static readonly ParseFunc<Ast> Literal =
        Or(NumericLiteral, Map(StringLiteral, x => new Ast(AstNodeName.StringConstantExpr, new Ast(AstNodeName.Value)
            {
                TextContent = x
            })
        ));

    private static readonly ParseFunc<Ast> ArgumentPlaceholder =
        Alias(new Ast(AstNodeName.ArgumentPlaceholder), "?");

    private static readonly ParseFunc<Ast> Argument =
        Or(ExprSingle, ArgumentPlaceholder);

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
            Not(FollowedMultiple(ReservedFunctionNames, new[] {Whitespace, Token("(")}),
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

    private static readonly ParseFunc<Ast> SequenceType = NotImplementedAst();

    private static readonly ParseFunc<Ast> ParenthesizedExpr = Or(
        Delimited(Token("("), Surrounded(Expr(), Whitespace), Token(")")),
        Map(Delimited(Token("("), Whitespace, Token(")")), _ => new Ast(AstNodeName.SequenceExpr))
    );

    // TODO: add others
    private static readonly ParseFunc<Ast> PrimaryExpr = Or(
        Literal,
        VarRef,
        ParenthesizedExpr,
        ContextItemExpr,
        FunctionCall
    );

    private static readonly ParseFunc<Ast[]> PostfixExprWithStep =
        Then(
            Map(PrimaryExpr, ParsingUtils.WrapInSequenceExprIfNeeded),
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
                var toWrap = new[] {expression};

                var predicates = new List<Ast>();
                var filters = new List<Ast>();

                var allowSinglePredicates = false;

                void FlushPredicates(bool allowSinglePred)
                {
                    if (allowSinglePred && predicates.Count == 1)
                        filters.Add(new Ast(AstNodeName.Predicate) {Children = new List<Ast> {predicates[0]}});
                    else if (predicates.Count != 0)
                        filters.Add(new Ast(AstNodeName.Predicates) {Children = predicates});
                    predicates.Clear();
                }

                void FlushFilters(bool ensureFilter, bool allowSinglePred)
                {
                    FlushPredicates(allowSinglePred);
                    if (filters.Count != 0)
                    {
                        if (toWrap[0].IsA(AstNodeName.SequenceExpr) &&
                            toWrap[0].Children.Count > 1)
                            toWrap = new[] {new Ast(AstNodeName.SequenceExpr, toWrap)};

                        toWrap = new[] {new Ast(AstNodeName.FilterExpr, toWrap)}.Concat(filters).ToArray();
                        filters.Clear();
                    }
                    else if (ensureFilter)
                    {
                        toWrap = new[] {new Ast(AstNodeName.FilterExpr, toWrap)};
                    }
                }

                foreach (var postfix in postfixExpr)
                    switch (postfix.Name)
                    {
                        case AstNodeName.Predicate:
                            predicates.Add(postfix.GetFirstChild()!);
                            break;
                        case AstNodeName.Lookup:
                            allowSinglePredicates = true;
                            FlushPredicates(allowSinglePredicates);
                            filters.Add(postfix);
                            break;
                        case AstNodeName.ArgumentList:
                            FlushFilters(false, allowSinglePredicates);
                            if (toWrap.Length > 1)
                                toWrap = new[]
                                {
                                    new Ast(AstNodeName.SequenceExpr,
                                        new Ast(AstNodeName.PathExpr, new Ast(AstNodeName.StepExpr, toWrap)))
                                };

                            toWrap = new[]
                            {
                                new Ast(AstNodeName.DynamicFunctionInvocationExpr, new[]
                                {
                                    new Ast(AstNodeName.FunctionItem, toWrap)
                                }.Concat(postfix.Children.Count > 0
                                    ? new[] {new Ast(AstNodeName.Arguments, postfix.Children.ToArray())}
                                    : Array.Empty<Ast>()).ToArray())
                            };
                            break;
                        default:
                            throw new Exception("Unreachable");
                    }

                FlushFilters(true, allowSinglePredicates);

                return toWrap;
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

    private static readonly ParseFunc<Ast> AbsoluteLocationPath =
        Or(Map(PrecededMultiple(new[] {Token("/"), Whitespace}, RelativePathExprWithForcedStep),
                path => new Ast(AstNodeName.PathExpr, new[] {new Ast(AstNodeName.RootExpr)}.Concat(path).ToArray())),
            Then(LocationPathAbbreviation, Preceded(Whitespace, RelativePathExprWithForcedStep),
                (abbrev, path) => new Ast(AstNodeName.PathExpr,
                    new[] {new Ast(AstNodeName.RootExpr), abbrev}.Concat(path).ToArray())),
            Map(Followed(Token("/"), Not(Preceded(Whitespace, Regex("[*a-zA-Z]")),
                    new[]
                    {
                        "Single rootExpr cannot be followed by something that can be interpreted as a relative path"
                    })),
                _ => new Ast(AstNodeName.PathExpr, new Ast(AstNodeName.RootExpr)))
        );

    private static readonly ParseFunc<Ast> PathExpr =
        Or(RelativePathExpr, AbsoluteLocationPath);

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
        Or(Map(EqName, x => x.GetAst(AstNodeName.EqName)),
            VarRef,
            ParenthesizedExpr
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
                    Alias(AstNodeName.IntersectOp, "intersect"),
                    Alias(AstNodeName.ExceptOp, "except")
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
                Followed(Alias(AstNodeName.IDivOp, "idiv"), AssertAdjacentOpeningTerminal),
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
            ValueCompare,
            NodeCompare,
            GeneralCompare
        ));

    private static readonly ParseFunc<Ast> AndExpr =
        BinaryOperator(ComparisonExpr,
            Followed(Alias(AstNodeName.AndOp, "and"), AssertAdjacentOpeningTerminal),
            DefaultBinaryOperatorFn);

    private static readonly ParseFunc<Ast> OrExpr =
        BinaryOperator(AndExpr,
            Followed(Alias(AstNodeName.OrOp, "or"), AssertAdjacentOpeningTerminal),
            DefaultBinaryOperatorFn);

    private static readonly ParseFunc<Ast> QueryBody =
        Map(Expr(), x => new Ast(AstNodeName.QueryBody, x));

    private static readonly ParseFunc<Ast> Prolog = NotImplementedAst();

    private static readonly ParseFunc<Ast> VersionDeclaration = NotImplementedAst();

    private static readonly ParseFunc<Ast> LibraryModule = NotImplementedAst();

    private static readonly ParseFunc<Ast> MainModule = Then(Optional(Prolog),
        Preceded(Whitespace, QueryBody),
        (prologPart, body) => new Ast(AstNodeName.MainModule, body)
    );

    private static readonly ParseFunc<Ast> Module = Then(
        Optional(Surrounded(VersionDeclaration, Whitespace)),
        Or(LibraryModule, MainModule),
        (versionDecl, modulePart) => new Ast(AstNodeName.Module,
            versionDecl != null ? new[] {versionDecl, modulePart} : new[] {modulePart})
    );

    private static ParseFunc<Ast> NotImplementedAst()
    {
        return Map(Token("NOT IMPLEMENTED WILL NEVER GET MATCHED"), s => new Ast(AstNodeName.NotImplemented));
    }

    private static ParseResult<Ast[]> RelativePathExprWithForcedStepIndirect(string input, int offset)
    {
        return RelativePathExprWithForcedStep(input, offset);
    }

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

    public static ParseResult<Ast> Parse(string input, ParseOptions options)
    {
        _options = options;
        return Complete(Surrounded(Module, Whitespace))(input, 0);
    }
}