using FontoXPathCSharp.Value;
using PrscSharp;
using static PrscSharp.PrscSharp;
using static FontoXPathCSharp.Parsing.ParsingFunctions;

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

public class XPathParser
{
    // Auxiliary Objects
    private readonly ParseOptions _options;
    private readonly Dictionary<int, Ast.StackTraceInfo> _stackTraceMap;
    private readonly WhitespaceParser _whitespaceParser;

    // Parser Combinators
    private readonly ParseFunc<Ast> AbbrevForwardStep;
    private readonly ParseFunc<Ast> AbbrevReverseStep;
    private readonly ParseFunc<Ast> AbsoluteLocationPath;
    private readonly ParseFunc<Ast> AdditiveExpr;
    private readonly ParseFunc<string> AllowingEmpty;
    private readonly ParseFunc<Ast> AndExpr;
    private readonly ParseFunc<Ast> Annotation;
    private readonly ParseFunc<Ast> Argument;
    private readonly ParseFunc<Ast[]> ArgumentList;
    private readonly ParseFunc<Ast> ArgumentPlaceholder;
    private readonly ParseFunc<Ast> ArrowExpr;
    private readonly ParseFunc<Ast> ArrowFunctionSpecifier;
    private readonly ParseFunc<Ast> AtomicOrUnionType;
    private readonly ParseFunc<QName> AttributeDeclaration;
    private readonly ParseFunc<QName> AttributeName;
    private readonly ParseFunc<Ast> AttributeTest;
    private readonly ParseFunc<Ast> AxisStep;
    private readonly ParseFunc<Ast> CastableExpr;
    private readonly ParseFunc<Ast> CastExpr;
    private readonly ParseFunc<Ast> ComparisonExpr;
    private readonly ParseFunc<Ast> DocumentTest;
    private readonly ParseFunc<QName> ElementDeclaration;
    private readonly ParseFunc<Ast> ElementTest;
    private readonly ParseFunc<Ast?> EnclosedExpr;
    private readonly ParseFunc<Ast> FlworExpr;
    private readonly ParseFunc<Ast> ForBinding;
    private readonly ParseFunc<Ast> ForClause;
    private readonly ParseFunc<Ast> ForwardStep;
    private readonly ParseFunc<Ast> FunctionBody;
    private readonly ParseFunc<Ast> FunctionCall;
    private readonly ParseFunc<Ast> FunctionItemExpr;
    private readonly ParseFunc<Ast> GroupByClause;
    private readonly ParseFunc<Ast> GroupingSpec;
    private readonly ParseFunc<Ast[]> GroupingSpecList;
    private readonly ParseFunc<Ast> GroupingVariable;
    private readonly ParseFunc<Ast> GroupVarInitialize;
    private readonly ParseFunc<Ast> IfExpr;
    private readonly ParseFunc<Ast> InitialClause;
    private readonly ParseFunc<Ast> InlineFunctionExpr;
    private readonly ParseFunc<Ast> InstanceOfExpr;
    private readonly ParseFunc<Ast> IntermediateClause;
    private readonly ParseFunc<Ast> IntersectExpr;
    private readonly ParseFunc<Ast> ItemType;
    private readonly ParseFunc<Ast> KindTest;
    private readonly ParseFunc<Ast> LetBinding;
    private readonly ParseFunc<Ast> LetClause;
    private readonly ParseFunc<Ast> LibraryModule;
    private readonly ParseFunc<Ast> Literal;
    private readonly ParseFunc<Ast> MainModule;
    private readonly ParseFunc<Ast> Module;
    private readonly ParseFunc<Ast> MultiplicativeExpr;
    private readonly ParseFunc<Ast> NamedFunctionRef;
    private readonly ParseFunc<Ast> NameTest;
    private readonly ParseFunc<Ast> NodeTest;
    private readonly ParseFunc<string> OccurenceIndicator;
    private readonly ParseFunc<Ast> OrderByClause;
    private readonly ParseFunc<Ast?> OrderModifier;
    private readonly ParseFunc<Ast> OrderSpec;
    private readonly ParseFunc<Ast[]> OrderSpecList;
    private readonly ParseFunc<Ast> OrExpr;
    private readonly ParseFunc<Ast> Param;
    private readonly ParseFunc<Ast[]> ParamList;
    private readonly ParseFunc<Ast> ParenthesizedExpr;
    private readonly ParseFunc<Ast> PathExpr;
    private readonly ParseFunc<Ast> PiTest;
    private readonly ParseFunc<Ast> PositionalVar;
    private readonly ParseFunc<Ast> PostfixExprWithoutStep;
    private readonly ParseFunc<Ast[]> PostfixExprWithStep;
    private readonly ParseFunc<Ast> Predicate;
    private readonly ParseFunc<Ast?> PredicateList;
    private readonly ParseFunc<Ast> PrimaryExpr;
    private readonly ParseFunc<Ast> Prolog;
    private readonly ParseFunc<Ast> QueryBody;
    private readonly ParseFunc<Ast> RangeExpr;
    private readonly ParseFunc<Ast> RelativePathExpr;
    private readonly ParseFunc<Ast[]> RelativePathExprWithForcedStep;
    private readonly ParseFunc<Ast> ReturnClause;
    private readonly ParseFunc<Ast> ReverseStep;
    private readonly ParseFunc<Ast> SchemaAttributeTest;
    private readonly ParseFunc<Ast> SchemaElementTest;
    private readonly ParseFunc<Ast[]> SequenceType;
    private readonly ParseFunc<Ast> SimpleMapExpr;
    private readonly ParseFunc<Ast> StepExprWithForcedStep;
    private readonly ParseFunc<Ast> StepExprWithoutStep;
    private readonly ParseFunc<Ast> StringConcatExpr;
    private readonly ParseFunc<string> StringLiteral;
    private readonly ParseFunc<Ast> TreatExpr;
    private readonly ParseFunc<Ast> TypeDeclaration;
    private readonly ParseFunc<Ast> UnaryExpr;
    private readonly ParseFunc<Ast> UnionExpr;
    private readonly ParseFunc<string> UriLiteral;
    private readonly ParseFunc<Ast> ValueExpr;
    private readonly ParseFunc<QName> VarName;
    private readonly ParseFunc<Ast> VersionDeclaration;
    private readonly ParseFunc<Ast> WhereClause;
    private readonly ParseFunc<Ast> Wildcard;

    private XPathParser(ParseOptions options)
    {
        _options = options;

        _whitespaceParser = new WhitespaceParser();
        var nameParser = new NameParser(_whitespaceParser);
        var literalParser = new LiteralParser(_whitespaceParser);
        var typesParser = new TypesParser(nameParser);

        _stackTraceMap = new Dictionary<int, Ast.StackTraceInfo>();
        var pathExprCache = new Dictionary<int, ParseResult<Ast>>();

        Predicate = Delimited(Token("["), Surrounded(Expr(), _whitespaceParser.Whitespace), Token("]"));

        StringLiteral = Map(_options.XQuery
                ? Or(
                    Surrounded(
                        Star(
                            Or(
                                literalParser.PredefinedEntityRef,
                                literalParser.CharRef,
                                literalParser.EscapeQuot,
                                Regex("[^\"&]"))
                        ),
                        Token("\"")
                    ),
                    Surrounded(Star(Or(
                        literalParser.PredefinedEntityRef,
                        literalParser.CharRef,
                        literalParser.EscapeApos,
                        Regex("[^'&]"))), Token("'")))
                : Or(Surrounded(Star(Or(literalParser.EscapeQuot, Regex("[^\"]"))), Token("\"")),
                    Surrounded(Star(Or(literalParser.EscapeApos, Regex("[^']"))), Token("'"))),
            x => string.Join("", x)
        );

        ElementTest = Or(
            Map(
                PrecededMultiple(new[] { Token("element"), _whitespaceParser.Whitespace },
                    Delimited(
                        Followed(Token("("), _whitespaceParser.Whitespace),
                        Then(
                            nameParser.ElementNameOrWildcard,
                            PrecededMultiple(
                                new[] { _whitespaceParser.Whitespace, Token(","), _whitespaceParser.Whitespace },
                                typesParser.TypeName),
                            (elemName, typeName) => (
                                nameOrWildcard: new Ast(AstNodeName.ElementName, elemName),
                                type: typeName.GetAst(AstNodeName.TypeName)
                            )
                        ),
                        Preceded(_whitespaceParser.Whitespace, Token(")"))
                    )
                ),
                x => new Ast(AstNodeName.ElementTest, x.nameOrWildcard, x.type)
            ),
            Map(
                PrecededMultiple(new[] { Token("element"), _whitespaceParser.Whitespace },
                    Delimited(
                        Token("("), nameParser.ElementNameOrWildcard, Token(")")
                    )
                ),
                nameOrWildcard => new Ast(AstNodeName.ElementTest, new Ast(AstNodeName.ElementName, nameOrWildcard))
            ),
            Map(
                PrecededMultiple(new[] { Token("element"), _whitespaceParser.Whitespace },
                    Delimited(
                        Token("("), _whitespaceParser.Whitespace, Token(")")
                    )
                ),
                _ => new Ast(AstNodeName.ElementTest)
            )
        );

        AttributeTest = Or(
            Map(
                PrecededMultiple(new[] { Token("attribute"), _whitespaceParser.Whitespace },
                    Delimited(
                        Followed(Token("("), _whitespaceParser.Whitespace),
                        Then(
                            nameParser.AttributeNameOrWildcard,
                            PrecededMultiple(
                                new[] { _whitespaceParser.Whitespace, Token(","), _whitespaceParser.Whitespace },
                                typesParser.TypeName),
                            (attrName, typeName) => (
                                nameOrWildcard: new Ast(AstNodeName.AttributeName, attrName),
                                type: typeName.GetAst(AstNodeName.TypeName)
                            )
                        ),
                        Preceded(_whitespaceParser.Whitespace, Token(")"))
                    )
                ),
                x => new Ast(AstNodeName.AttributeTest, x.nameOrWildcard, x.type)
            ),
            Map(
                PrecededMultiple(new[] { Token("attribute"), _whitespaceParser.Whitespace },
                    Delimited(
                        Token("("), nameParser.AttributeNameOrWildcard, Token(")")
                    )
                ),
                nameOrWildcard => new Ast(AstNodeName.AttributeTest, new Ast(AstNodeName.AttributeName, nameOrWildcard))
            ),
            Map(
                PrecededMultiple(new[] { Token("attribute"), _whitespaceParser.Whitespace },
                    Delimited(
                        Token("("), _whitespaceParser.Whitespace, Token(")")
                    )
                ),
                _ => new Ast(AstNodeName.AttributeTest)
            )
        );

        ElementDeclaration = nameParser.ElementName;

        SchemaElementTest = Map(
            Delimited(
                Token("schema-element("),
                Surrounded(ElementDeclaration, _whitespaceParser.Whitespace),
                Token(")")
            ),
            x => x.GetAst(AstNodeName.SchemaElementTest)
        );

        AttributeName = nameParser.EqName;

        AttributeDeclaration = AttributeName;

        SchemaAttributeTest = Map(
            Delimited(
                Token("schema-attribute("),
                Surrounded(AttributeDeclaration, _whitespaceParser.Whitespace),
                Token(")")
            ),
            decl => decl.GetAst(AstNodeName.SchemaAttributeTest)
        );

        PiTest = Or(
            Map(
                Delimited(
                    Token("processing-instruction("),
                    Surrounded(Or(nameParser.NcName, StringLiteral), _whitespaceParser.Whitespace),
                    Token(")")
                ),
                target => new Ast(AstNodeName.PiTest, new Ast(AstNodeName.PiTarget)
                {
                    TextContent = target
                })
            ),
            Alias(new Ast(AstNodeName.PiTest), "processing-instruction()")
        );

        DocumentTest = Map(
            Delimited(Token("document-node("),
                Surrounded(Optional(Or(ElementTest, SchemaElementTest)), _whitespaceParser.Whitespace),
                Token(")")
            ),
            x => x == null ? new Ast(AstNodeName.DocumentTest) : new Ast(AstNodeName.DocumentTest, x)
        );

        KindTest = Or(
            DocumentTest,
            ElementTest,
            AttributeTest,
            SchemaElementTest,
            SchemaAttributeTest,
            PiTest,
            literalParser.CommentTest,
            literalParser.TextTest,
            literalParser.NamespaceNodeTest,
            literalParser.AnyKindTest
        );

        Wildcard = Or(
            Map(Preceded(Token("*:"), nameParser.NcName),
                x => new Ast(AstNodeName.Wildcard, new Ast(AstNodeName.Star),
                    new Ast(AstNodeName.NcName) { TextContent = x })
            ),
            Alias(new Ast(AstNodeName.Wildcard), "*"),
            Map(Followed(nameParser.BracedUriLiteral, Token("*")),
                x => new Ast(AstNodeName.Wildcard, new Ast(AstNodeName.Uri) { TextContent = x },
                    new Ast(AstNodeName.Star))),
            Map(Followed(nameParser.NcName, Token(":*")), x =>
                new Ast(AstNodeName.Wildcard, new Ast(AstNodeName.NcName) { TextContent = x },
                    new Ast(AstNodeName.Star))
            )
        );

        NameTest = Or(
            Wildcard,
            Map(nameParser.EqName, x => new Ast(AstNodeName.NameTest)
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

        NodeTest = Or(KindTest, NameTest);

        AbbrevForwardStep = Then(Optional(Token("@")), NodeTest,
            (a, b) => new Ast(AstNodeName.StepExpr, new Ast(AstNodeName.XPathAxis)
            {
                TextContent = a != null || b.IsA(AstNodeName.AttributeTest, AstNodeName.SchemaAttributeTest)
                    ? "attribute"
                    : "child"
            }, b));

        ForwardStep = Or(Then(literalParser.ForwardAxis, NodeTest,
                (axis, test) =>
                    new Ast(AstNodeName.StepExpr,
                        new Ast(AstNodeName.XPathAxis) { TextContent = axis },
                        test
                    )),
            AbbrevForwardStep);

        AbbrevReverseStep = Map(Token(".."), _ =>
            new Ast(AstNodeName.StepExpr, new Ast(AstNodeName.XPathAxis) { TextContent = "parent" },
                new Ast(AstNodeName.AnyKindTest))
        );

        ReverseStep = Or(
            Then(literalParser.ReverseAxis, NodeTest,
                (axis, test) =>
                    new Ast(AstNodeName.StepExpr,
                        new Ast(AstNodeName.XPathAxis) { TextContent = axis },
                        test
                    )),
            AbbrevReverseStep
        );

        PredicateList = Map(Star(Preceded(_whitespaceParser.Whitespace, Predicate)),
            x => x.Length > 0 ? new Ast(AstNodeName.Predicates, x) : null);

        AxisStep = Then(
            Or(ReverseStep, ForwardStep),
            PredicateList,
            (a, b) =>
            {
                if (b == null) return a;
                a.Children.Add(b);
                return a;
            }
        );

        Literal = Or(literalParser.NumericLiteral, Map(StringLiteral, x => new Ast(AstNodeName.StringConstantExpr,
            new Ast(AstNodeName.Value)
            {
                TextContent = x
            })
        ));

        ArgumentPlaceholder = Alias(new Ast(AstNodeName.ArgumentPlaceholder), "?");

        Argument = Or(ExprSingle, ArgumentPlaceholder);

        ArgumentList = Map(
            Delimited(
                Token("("),
                Surrounded(
                    Optional(
                        Then(Argument,
                            Star(Preceded(Surrounded(Token(","), _whitespaceParser.Whitespace), Argument)),
                            (first, following) => following.Prepend(first).ToArray())
                    ),
                    _whitespaceParser.Whitespace
                ),
                Token(")")
            ),
            x => x ?? Array.Empty<Ast>()
        );

        FunctionCall = Preceded(
            Not(
                FollowedMultiple(literalParser.ReservedFunctionNames,
                    new[] { _whitespaceParser.Whitespace, Token("(") }),
                new[] { "cannot use reserved keyword for function names" }),
            Then(nameParser.EqName, Preceded(_whitespaceParser.Whitespace, ArgumentList),
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

        AtomicOrUnionType = Map(nameParser.EqName, x => x.GetAst(AstNodeName.AtomicType));

        OccurenceIndicator = Or(Token("?"), Token("*"), Token("+"));

        NamedFunctionRef = Then(
            nameParser.EqName,
            Preceded(Token("#"), literalParser.IntegerLiteral),
            (functionName, integer) => new Ast(AstNodeName.NamedFunctionRef,
                functionName.GetAst(AstNodeName.FunctionName), integer)
        );

        EnclosedExpr = Delimited(
            Token("{"),
            Surrounded(Optional(Expr()), _whitespaceParser.Whitespace),
            Token("}")
        );

        FunctionBody = Map(EnclosedExpr, x => x ?? new Ast(AstNodeName.SequenceExpr));

        SequenceType = Or(
            Map(Token("empty-sequence()"), _ => new[] { new Ast(AstNodeName.VoidSequenceType) }),
            Then(
                ItemTypeIndirect,
                Optional(Preceded(_whitespaceParser.Whitespace, OccurenceIndicator)),
                (type, occurrence) =>
                    new[] { type }
                        .Concat(occurrence != null
                            ? new[] { new Ast(AstNodeName.OccurrenceIndicator) { TextContent = occurrence } }
                            : Array.Empty<Ast>())
                        .ToArray())
        );

        Annotation = Then(
            PrecededMultiple(new[] { Token("%"), _whitespaceParser.Whitespace }, Literal),
            Optional(
                Followed(
                    Then(
                        PrecededMultiple(new[] { Token("("), _whitespaceParser.Whitespace }, Literal),
                        Star(PrecededMultiple(new[] { Token(","), _whitespaceParser.Whitespace }, Literal)),
                        (lhs, rhs) => ParsingUtils.WrapSingleton(lhs).Concat(rhs)
                    ),
                    Token(")")
                )
            ),
            (annotationName, parameters) =>
                new Ast(AstNodeName.Annotation,
                    ParsingUtils.WrapSingleton(new Ast(AstNodeName.AnnotationName, annotationName))
                        .Concat(ParsingUtils.IfNotNullWrapOther(parameters,
                            new Ast(AstNodeName.Arguments, parameters))))
        );

        ItemType = Or(
            KindTest,
            Map(Alias(AstNodeName.AnyItemType, "item()"), name => new Ast(name)),
            AtomicOrUnionType
        );

        TypeDeclaration = Map(
            PrecededMultiple(new[] { Token("as"), _whitespaceParser.WhitespacePlus }, SequenceType),
            x => new Ast(AstNodeName.TypeDeclaration, x)
        );

        Param = Then(
            Preceded(Token("$"), nameParser.EqName),
            Optional(Preceded(_whitespaceParser.WhitespacePlus, TypeDeclaration)),
            (variableName, typeDecl) => new Ast(
                AstNodeName.Param,
                new[] { variableName.GetAst(AstNodeName.VarName) }.Concat(ParsingUtils.WrapNullableInArray(typeDecl))
            )
        );

        ParamList = BinaryOperator(
            Param,
            Alias(AstNodeName.Arguments, ","),
            (lhs, rhs) => new[] { lhs }.Concat(rhs.Select(x => x.Item2)).ToArray()
        );


        InlineFunctionExpr = Then4(
            Star(Annotation),
            PrecededMultiple(
                new[]
                {
                    _whitespaceParser.Whitespace, Token("function"), _whitespaceParser.Whitespace, Token("("),
                    _whitespaceParser.Whitespace
                },
                Optional(ParamList)
            ),
            PrecededMultiple(new[] { _whitespaceParser.Whitespace, Token(")"), _whitespaceParser.Whitespace },
                Optional(
                    Map(
                        PrecededMultiple(new[] { Token("as"), _whitespaceParser.Whitespace },
                            Followed(SequenceType, _whitespaceParser.Whitespace)),
                        x => new Ast(AstNodeName.TypeDeclaration, x)
                    )
                )
            ),
            FunctionBody,
            (annotations, parameters, typeDecl, body) =>
                new Ast(AstNodeName.InlineFunctionExpr, annotations)
                    .AddChild(new Ast(AstNodeName.ParamList, parameters ?? Array.Empty<Ast>()))
                    .AddChildren(ParsingUtils.WrapNullableInArray(typeDecl))
                    .AddChild(new Ast(AstNodeName.FunctionBody, body))
        );

        FunctionItemExpr = Or(NamedFunctionRef, InlineFunctionExpr);

        VarName = nameParser.EqName;

        LetBinding = Then3(
            Preceded(Token("$"), VarName),
            Preceded(_whitespaceParser.Whitespace, Optional(TypeDeclaration)),
            Preceded(Surrounded(Token(":="), _whitespaceParser.Whitespace), ExprSingle),
            (variableName, typeDecl, letExpr) =>
            {
                return new Ast(AstNodeName.LetClauseItem,
                    new Ast(AstNodeName.TypedVariableBinding,
                        new[] { variableName.GetAst(AstNodeName.VarName) }
                            .Concat(ParsingUtils.WrapNullableInArray(typeDecl))),
                    new Ast(AstNodeName.LetExpr, letExpr));
            }
        );

        AllowingEmpty = Delimited(Token("allowing"), _whitespaceParser.WhitespacePlus, Token("empty"));

        PositionalVar = Map(
            PrecededMultiple(new[] { Token("at"), _whitespaceParser.WhitespacePlus, Token("$") }, VarName),
            x => x.GetAst(AstNodeName.PositionalVariableBinding)
        );

        ForBinding = Then5(
            Preceded(Token("$"), VarName),
            Preceded(_whitespaceParser.Whitespace, Optional(TypeDeclaration)),
            Preceded(_whitespaceParser.Whitespace, Optional(AllowingEmpty)),
            Preceded(_whitespaceParser.Whitespace, Optional(PositionalVar)),
            Preceded(Surrounded(Token("in"), _whitespaceParser.Whitespace), ExprSingle),
            (variableName, typeDecl, empty, pos, forExpr) =>
                new Ast(AstNodeName.ForClauseItem,
                    new[]
                        {
                            new Ast(AstNodeName.TypedVariableBinding,
                                variableName
                                    .GetAst(AstNodeName.VarName)
                                    .AddChildren(ParsingUtils.WrapNullableInArray(typeDecl))
                            )
                        }
                        .Concat(empty != null ? new[] { new Ast(AstNodeName.AllowingEmpty) } : Array.Empty<Ast>())
                        .Concat(ParsingUtils.WrapNullableInArray(pos))
                        .Concat(new[] { new Ast(AstNodeName.ForExpr, forExpr) })
                )
        );

        ForClause = PrecededMultiple(
            new[] { Token("for"), _whitespaceParser.WhitespacePlus },
            BinaryOperator(
                ForBinding,
                Alias(AstNodeName.SequenceExpr, ","),
                (lhs, rhs) =>
                    new Ast(AstNodeName.ForClause, new[] { lhs }.Concat(rhs.Select(x => x.Item2)))));

        LetClause = Map(
            PrecededMultiple(
                new[] { Token("let"), _whitespaceParser.Whitespace },
                BinaryOperator(
                    LetBinding,
                    Alias(AstNodeName.Arguments, ","),
                    (lhs, rhs) => new[] { lhs }.Concat(rhs.Select(e => e.Item2)))),
            x => new Ast(AstNodeName.LetClause, x)
        );

        InitialClause = Or(ForClause, LetClause);

        WhereClause = Map(
            PrecededMultiple(
                new[] { Token("where"), literalParser.AssertAdjacentOpeningTerminal, _whitespaceParser.Whitespace },
                ExprSingle),
            x => new Ast(AstNodeName.WhereClause, x)
        );

        UriLiteral = StringLiteral;

        GroupingVariable = Map(
            Preceded(Token("$"), VarName),
            x => x.GetAst(AstNodeName.VarName)
        );

        GroupVarInitialize = Then(
            Preceded(_whitespaceParser.Whitespace, Optional(TypeDeclaration)),
            Preceded(Surrounded(Token(":="), _whitespaceParser.Whitespace), ExprSingle),
            (t, val) => new Ast(
                AstNodeName.GroupVarInitialize,
                (t != null ? new[] { new Ast(AstNodeName.TypeDeclaration, t) } : Array.Empty<Ast>())
                .Concat(new[] { new Ast(AstNodeName.VarValue, val) }))
        );

        GroupingSpec = Then3(
            GroupingVariable,
            Optional(GroupVarInitialize),
            Optional(Map(Preceded(Surrounded(Token("collation"), _whitespaceParser.Whitespace), UriLiteral),
                x => new Ast(AstNodeName.Collation) { TextContent = x })
            ),
            (variableName, init, col) => new Ast(
                AstNodeName.GroupingSpec,
                new[] { variableName }
                    .Concat(ParsingUtils.WrapNullableInArray(init))
                    .Concat(ParsingUtils.WrapNullableInArray(col))
            )
        );

        GroupingSpecList = BinaryOperator(
            GroupingSpec,
            Alias(AstNodeName.Arguments, ","),
            (lhs, rhs) => new[] { lhs }.Concat(rhs.Select(x => x.Item2)).ToArray()
        );

        GroupByClause = Map(
            PrecededMultiple(
                new[] { Token("group"), _whitespaceParser.WhitespacePlus, Token("by"), _whitespaceParser.Whitespace },
                GroupingSpecList),
            x => new Ast(AstNodeName.GroupByClause, x)
        );

        OrderModifier = Then3(
            Optional(Or(Token("ascending"), Token("descending"))),
            Optional(PrecededMultiple(
                new[] { _whitespaceParser.Whitespace, Token("empty"), _whitespaceParser.Whitespace },
                Or(new[] { Token("greatest"), Token("least") }.Select(x => Map(x, y => "empty " + y)).ToArray()))),
            Preceded(_whitespaceParser.Whitespace,
                Optional(PrecededMultiple(new[] { Token("collation"), _whitespaceParser.Whitespace }, UriLiteral))),
            (kind, empty, collation) =>
                kind == null && empty == null && collation == null
                    ? null
                    : new Ast(AstNodeName.OrderModifier,
                        (kind != null
                            ? new[] { new Ast(AstNodeName.OrderingKind) { TextContent = kind } }
                            : Array.Empty<Ast>())
                        .Concat(empty != null
                            ? new[] { new Ast(AstNodeName.EmptyOrderingMode) { TextContent = empty } }
                            : Array.Empty<Ast>())
                        .Concat(collation != null
                            ? new[] { new Ast(AstNodeName.Collation) { TextContent = collation } }
                            : Array.Empty<Ast>())));

        OrderSpec = Then(
            ExprSingle,
            Preceded(_whitespaceParser.Whitespace, OrderModifier),
            (orderByExpr, modifier) =>
                new Ast(AstNodeName.OrderBySpec,
                    new[] { new Ast(AstNodeName.OrderByExpr, orderByExpr) }
                        .Concat(ParsingUtils.WrapNullableInArray(modifier))
                )
        );

        OrderSpecList = BinaryOperator(
            OrderSpec,
            Alias(AstNodeName.Arguments, ","),
            (lhs, rhs) =>
                new[] { lhs }
                    .Concat(rhs.Select(x => x.Item2))
                    .ToArray()
        );

        OrderByClause = Then(
            Or(
                Map(
                    PrecededMultiple(new[] { Token("order"), _whitespaceParser.WhitespacePlus }, Token("by")),
                    _ => false),
                Map(
                    PrecededMultiple(
                        new[]
                        {
                            Token("stable"), _whitespaceParser.WhitespacePlus, Token("order"),
                            _whitespaceParser.WhitespacePlus
                        },
                        Token("by")),
                    _ => true)
            ),
            Preceded(_whitespaceParser.Whitespace, OrderSpecList),
            (stable, specList) =>
                new Ast(AstNodeName.OrderByClause,
                    (stable ? new[] { new Ast(AstNodeName.Stable) } : Array.Empty<Ast>())
                    .Concat(specList))
        );

        IntermediateClause = Or(
            InitialClause,
            WhereClause,
            GroupByClause,
            OrderByClause
        );

        ReturnClause = Map(
            PrecededMultiple(new[] { Token("return"), _whitespaceParser.Whitespace }, ExprSingle),
            x => new Ast(AstNodeName.ReturnClause, x)
        );

        FlworExpr = Then3(
            InitialClause,
            Star(Preceded(_whitespaceParser.Whitespace, IntermediateClause)),
            Preceded(_whitespaceParser.Whitespace, ReturnClause),
            (initial, intermediate, ret) => new Ast(AstNodeName.FlworExpr,
                new[] { initial }.Concat(intermediate).Concat(new[] { ret }))
        );

        ParenthesizedExpr = Or(
            Delimited(Token("("), Surrounded(Expr(), _whitespaceParser.Whitespace), Token(")")),
            Map(Delimited(Token("("), _whitespaceParser.Whitespace, Token(")")), _ => new Ast(AstNodeName.SequenceExpr))
        );

        PrimaryExpr = Or(
            Literal,
            nameParser.VarRef,
            ParenthesizedExpr,
            literalParser.ContextItemExpr,
            FunctionCall,
            FunctionItemExpr
        );

        PostfixExprWithStep = Then(
            Map(PrimaryExpr, ParsingUtils.WrapInSequenceExprIfNeeded),
            Star(
                Or(
                    Map(Preceded(_whitespaceParser.Whitespace, Predicate),
                        x => new Ast(AstNodeName.Predicate, x)),
                    Map(Preceded(_whitespaceParser.Whitespace, ArgumentList),
                        x => new Ast(AstNodeName.ArgumentList, x))
                    // TODO: Preceded(WhitespaceParser.Whitespace, Lookup()),
                )
            ),
            (expression, postfixExpr) =>
            {
                var toWrap = new[] { expression };

                var predicates = new List<Ast>();
                var filters = new List<Ast>();

                var allowSinglePredicates = false;

                void FlushPredicates(bool allowSinglePred)
                {
                    if (allowSinglePred && predicates.Count == 1)
                        filters.Add(new Ast(AstNodeName.Predicate) { Children = new List<Ast> { predicates[0] } });
                    else if (predicates.Count != 0)
                        filters.Add(new Ast(AstNodeName.Predicates) { Children = predicates });
                    predicates.Clear();
                }

                void FlushFilters(bool ensureFilter, bool allowSinglePred)
                {
                    FlushPredicates(allowSinglePred);
                    if (filters.Count != 0)
                    {
                        if (toWrap[0].IsA(AstNodeName.SequenceExpr) &&
                            toWrap[0].Children.Count > 1)
                            toWrap = new[] { new Ast(AstNodeName.SequenceExpr, toWrap) };

                        toWrap = new[] { new Ast(AstNodeName.FilterExpr, toWrap) }.Concat(filters).ToArray();
                        filters.Clear();
                    }
                    else if (ensureFilter)
                    {
                        toWrap = new[] { new Ast(AstNodeName.FilterExpr, toWrap) };
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
                                    ? new[] { new Ast(AstNodeName.Arguments, postfix.Children.ToArray()) }
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

        StepExprWithForcedStep = Or(
            Map(PostfixExprWithStep, x => new Ast(AstNodeName.StepExpr, x)),
            AxisStep
        );

        PostfixExprWithoutStep = Followed(
            PrimaryExpr,
            Peek(
                // TODO: add lookup
                Not(
                    Preceded(_whitespaceParser.Whitespace,
                        Or(Predicate, Map(ArgumentList, _ => new Ast(AstNodeName.All)))),
                    new[]
                    {
                        "Primary expression not followed by predicate, argumentList, or lookup"
                    })
            )
        );

        StepExprWithoutStep = PostfixExprWithoutStep;

        RelativePathExpr = Or(
            Then3(StepExprWithForcedStep,
                Preceded(_whitespaceParser.Whitespace, literalParser.LocationPathAbbreviation),
                Preceded(_whitespaceParser.Whitespace, RelativePathExprWithForcedStepIndirect),
                (lhs, abbrev, rhs) => new Ast(AstNodeName.PathExpr, new[] { lhs, abbrev }.Concat(rhs).ToArray())),
            Then(
                StepExprWithForcedStep,
                Preceded(Surrounded(Token("/"), _whitespaceParser.Whitespace), RelativePathExprWithForcedStepIndirect),
                (lhs, rhs) => new Ast(AstNodeName.PathExpr, new[] { lhs }.Concat(rhs).ToArray())),
            StepExprWithoutStep,
            Map(
                StepExprWithForcedStep, x =>
                    new Ast(AstNodeName.PathExpr, x)
            )
        );

        RelativePathExprWithForcedStep = Or(
            Then3(
                StepExprWithForcedStep,
                Preceded(_whitespaceParser.Whitespace, literalParser.LocationPathAbbreviation),
                Preceded(_whitespaceParser.Whitespace, RelativePathExprWithForcedStepIndirect),
                (lhs, abbrev, rhs) => new[] { lhs, abbrev }.Concat(rhs).ToArray()
            ),
            Then(
                StepExprWithForcedStep,
                Preceded(Surrounded(Token("/"), _whitespaceParser.Whitespace), RelativePathExprWithForcedStepIndirect),
                (lhs, rhs) => new[] { lhs }.Concat(rhs).ToArray()), Map(StepExprWithForcedStep, x => new[] { x }),
            Map(StepExprWithForcedStep, x => new[] { x })
        );

        AbsoluteLocationPath = Or(Map(
                PrecededMultiple(new[] { Token("/"), _whitespaceParser.Whitespace }, RelativePathExprWithForcedStep),
                path => new Ast(AstNodeName.PathExpr, new[] { new Ast(AstNodeName.RootExpr) }.Concat(path).ToArray())),
            Then(literalParser.LocationPathAbbreviation,
                Preceded(_whitespaceParser.Whitespace, RelativePathExprWithForcedStep),
                (abbrev, path) => new Ast(AstNodeName.PathExpr,
                    new[] { new Ast(AstNodeName.RootExpr), abbrev }.Concat(path).ToArray())),
            Map(Followed(Token("/"), Not(Preceded(_whitespaceParser.Whitespace, Regex("[*a-zA-Z]")),
                    new[]
                    {
                        "Single rootExpr cannot be followed by something that can be interpreted as a relative path"
                    })),
                _ => new Ast(AstNodeName.PathExpr, new Ast(AstNodeName.RootExpr)))
        );

        PathExpr = Cached(
            Or(RelativePathExpr, AbsoluteLocationPath),
            pathExprCache
        );
        
        SimpleMapExpr = WrapInStackTrace(
            BinaryOperator(PathExpr, Alias(AstNodeName.SimpleMapExpr, "!"), (lhs, rhs) =>
                rhs.Length == 0
                    ? lhs
                    : new Ast(AstNodeName.SimpleMapExpr,
                        lhs.Name == AstNodeName.PathExpr
                            ? lhs
                            : new Ast(AstNodeName.PathExpr,
                                new Ast(AstNodeName.StepExpr,
                                    new Ast(AstNodeName.FilterExpr, ParsingUtils.WrapInSequenceExprIfNeeded(lhs))
                                )
                            )
                    ).AddChildren(
                        rhs.Select(value =>
                        {
                            var item = value.Item2;
                            return item.Name == AstNodeName.PathExpr
                                ? item
                                : new Ast(AstNodeName.PathExpr,
                                    new Ast(AstNodeName.StepExpr,
                                        new Ast(AstNodeName.FilterExpr, ParsingUtils.WrapInSequenceExprIfNeeded(item))
                                    )
                                );
                        })
                    )
            )
        );

        ValueExpr = Or(
            // TODO: ValidateExpr(),
            // TODO: ExtensionExpr(),
            SimpleMapExpr,
            PathExpr
        );

        UnaryExpr = Or(
            Then(
                Or(
                    Alias(AstNodeName.UnaryMinusOp, "-"),
                    Alias(AstNodeName.UnaryPlusOp, "+")
                ),
                Preceded(_whitespaceParser.Whitespace, UnaryExprIndirect),
                (op, value) => new Ast(op, new Ast(AstNodeName.Operand, value))
            ),
            ValueExpr
        );

        ArrowFunctionSpecifier = Or(Map(nameParser.EqName, x => x.GetAst(AstNodeName.EqName)),
            nameParser.VarRef,
            ParenthesizedExpr
        );

        ArrowExpr = Then(
            UnaryExpr,
            Star(
                PrecededMultiple(
                    new[]
                    {
                        _whitespaceParser.Whitespace, Token("=>"), _whitespaceParser.Whitespace
                    },
                    Then(
                        ArrowFunctionSpecifier,
                        Preceded(_whitespaceParser.Whitespace, ArgumentList),
                        (specifier, argList) => (specifier, argList)
                    )
                )
            ),
            (Ast argExpr, (Ast, Ast[])[] functionParts) => functionParts.Aggregate(argExpr,
                (arg, part) => new Ast(AstNodeName.ArrowExpr, new Ast(AstNodeName.ArgExpr, arg), part.Item1,
                    new Ast(AstNodeName.Arguments, part.Item2)))
        );

        CastExpr = Then(
            ArrowExpr,
            Optional(
                PrecededMultiple(
                    new[]
                    {
                        _whitespaceParser.Whitespace,
                        Token("cast"),
                        _whitespaceParser.WhitespacePlus,
                        Token("as"),
                        literalParser.AssertAdjacentOpeningTerminal,
                        _whitespaceParser.Whitespace
                    },
                    typesParser.SingleType)
            ),
            (lhs, rhs) =>
                rhs != null
                    ? new Ast(AstNodeName.CastExpr, new Ast(AstNodeName.ArgExpr, lhs),
                        rhs)
                    : lhs
        );

        CastableExpr = Then(
            CastExpr,
            Optional(
                PrecededMultiple(
                    new[]
                    {
                        _whitespaceParser.Whitespace,
                        Token("castable"),
                        _whitespaceParser.WhitespacePlus,
                        Token("as"),
                        literalParser.AssertAdjacentOpeningTerminal,
                        _whitespaceParser.Whitespace
                    },
                    typesParser.SingleType)
            ),
            (lhs, rhs) =>
                rhs != null
                    ? new Ast(AstNodeName.CastableExpr, new Ast(AstNodeName.ArgExpr, lhs),
                        rhs)
                    : lhs
        );

        TreatExpr = Then(
            CastableExpr,
            Optional(
                PrecededMultiple(
                    new[]
                    {
                        _whitespaceParser.Whitespace,
                        Token("treat"),
                        _whitespaceParser.WhitespacePlus,
                        Token("as"),
                        literalParser.AssertAdjacentOpeningTerminal,
                        _whitespaceParser.Whitespace
                    },
                    SequenceType)
            ),
            (lhs, rhs) =>
                rhs != null
                    ? new Ast(AstNodeName.TreatExpr, new Ast(AstNodeName.ArgExpr, lhs),
                        new Ast(AstNodeName.SequenceType, rhs))
                    : lhs
        );

        InstanceOfExpr = Then(
            TreatExpr,
            Optional(
                PrecededMultiple(new[]
                    {
                        _whitespaceParser.Whitespace,
                        Token("instance"),
                        _whitespaceParser.WhitespacePlus,
                        Token("of"),
                        literalParser.AssertAdjacentOpeningTerminal,
                        _whitespaceParser.Whitespace
                    },
                    SequenceType)
            ),
            (lhs, rhs) =>
                rhs != null
                    ? new Ast(AstNodeName.InstanceOfExpr, new Ast(AstNodeName.ArgExpr, lhs),
                        new Ast(AstNodeName.SequenceType, rhs))
                    : lhs
        );

        IfExpr = Then(
            Then(
                PrecededMultiple(new[]
                {
                    Token("if"),
                    _whitespaceParser.Whitespace,
                    Token("("),
                    _whitespaceParser.Whitespace
                }, Expr()),
                PrecededMultiple(new[]
                {
                    _whitespaceParser.Whitespace,
                    Token(")"),
                    _whitespaceParser.Whitespace,
                    Token("then"),
                    literalParser.AssertAdjacentOpeningTerminal,
                    _whitespaceParser.Whitespace
                }, ExprSingle),
                (ifClause, thenClause) => new[] { ifClause, thenClause }
            ),
            PrecededMultiple(new[]
            {
                _whitespaceParser.Whitespace,
                Token("else"),
                literalParser.AssertAdjacentOpeningTerminal,
                _whitespaceParser.Whitespace
            }, ExprSingle),
            (ifThen, elseClause) =>
                new Ast(AstNodeName.IfThenElseExpr,
                    new Ast(AstNodeName.IfClause, ifThen[0]),
                    new Ast(AstNodeName.ThenClause, ifThen[1]),
                    new Ast(AstNodeName.ElseClause, elseClause))
        );

        IntersectExpr = BinaryOperator(
            InstanceOfExpr,
            Followed(
                Or(
                    Alias(AstNodeName.IntersectOp, "intersect"),
                    Alias(AstNodeName.ExceptOp, "except")
                ),
                literalParser.AssertAdjacentOpeningTerminal
            ),
            DefaultBinaryOperatorFn
        );

        UnionExpr = BinaryOperator(IntersectExpr,
            Or(
                Alias(AstNodeName.UnionOp, "|"),
                Followed(Alias(AstNodeName.UnionOp, "union"), literalParser.AssertAdjacentOpeningTerminal)
            ),
            DefaultBinaryOperatorFn
        );

        MultiplicativeExpr = BinaryOperator(
            UnionExpr,
            Or(
                Alias(AstNodeName.MultiplyOp, "*"),
                Followed(Alias(AstNodeName.DivOp, "div"), literalParser.AssertAdjacentOpeningTerminal),
                Followed(Alias(AstNodeName.IDivOp, "idiv"), literalParser.AssertAdjacentOpeningTerminal),
                Followed(Alias(AstNodeName.ModOp, "mod"), literalParser.AssertAdjacentOpeningTerminal)
            ),
            DefaultBinaryOperatorFn
        );

        AdditiveExpr = BinaryOperator(MultiplicativeExpr, Or(
            Alias(AstNodeName.SubtractOp, "-"),
            Alias(AstNodeName.AddOp, "+")
        ), DefaultBinaryOperatorFn);

        RangeExpr = NonRepeatableBinaryOperator(AdditiveExpr,
            Followed(Alias(AstNodeName.RangeSequenceExpr, "to"), literalParser.AssertAdjacentOpeningTerminal),
            AstNodeName.StartExpr,
            AstNodeName.EndExpr);

        StringConcatExpr =
            BinaryOperator(RangeExpr, Alias(AstNodeName.StringConcatenateOp, "||"), DefaultBinaryOperatorFn);

        ComparisonExpr = NonRepeatableBinaryOperator(StringConcatExpr, Or(
            literalParser.ValueCompare,
            literalParser.NodeCompare,
            literalParser.GeneralCompare
        ));

        AndExpr = BinaryOperator(ComparisonExpr,
            Followed(Alias(AstNodeName.AndOp, "and"), literalParser.AssertAdjacentOpeningTerminal),
            DefaultBinaryOperatorFn);

        OrExpr = BinaryOperator(AndExpr,
            Followed(Alias(AstNodeName.OrOp, "or"), literalParser.AssertAdjacentOpeningTerminal),
            DefaultBinaryOperatorFn);

        QueryBody = Map(Expr(), x => new Ast(AstNodeName.QueryBody, x));

        Prolog = NotImplementedAst();

        VersionDeclaration = NotImplementedAst();

        LibraryModule = NotImplementedAst();

        MainModule = Then(Optional(Prolog),
            Preceded(_whitespaceParser.Whitespace, QueryBody),
            (prologPart, body) => new Ast(AstNodeName.MainModule, body)
        );

        Module = Then(
            Optional(Surrounded(VersionDeclaration, _whitespaceParser.Whitespace)),
            Or(LibraryModule, MainModule),
            (versionDecl, modulePart) => new Ast(AstNodeName.Module,
                versionDecl != null ? new[] { versionDecl, modulePart } : new[] { modulePart })
        );
    }

    private ParseResult<Ast> ItemTypeIndirect(string input, int offset)
    {
        return ItemType(input, offset);
    }

    private static ParseFunc<Ast> NotImplementedAst()
    {
        return Map(Token("NOT IMPLEMENTED WILL NEVER GET MATCHED"), _ => new Ast(AstNodeName.NotImplemented));
    }

    private ParseResult<Ast[]> RelativePathExprWithForcedStepIndirect(string input, int offset)
    {
        return RelativePathExprWithForcedStep(input, offset);
    }

    private Ast DefaultBinaryOperatorFn(Ast lhs, IEnumerable<(AstNodeName, Ast)> rhs)
    {
        return rhs.Aggregate(lhs, (lh, rh) =>
            new Ast(rh.Item1, new Ast(AstNodeName.FirstOperand, lh), new Ast(AstNodeName.SecondOperand, rh.Item2)));
    }

    private ParseFunc<TS> BinaryOperator<T, TS>(ParseFunc<T> expr,
        ParseFunc<AstNodeName> op,
        Func<T, (AstNodeName, T)[], TS> constructionFn)
    {
        return Then(
            expr,
            Star(Then(Surrounded(op, _whitespaceParser.Whitespace), expr, (a, b) => (a, b))),
            constructionFn
        );
    }

    private ParseFunc<Ast> NonRepeatableBinaryOperator(ParseFunc<Ast> expr,
        ParseFunc<AstNodeName> op,
        AstNodeName firstArgName = AstNodeName.FirstOperand,
        AstNodeName secondArgName = AstNodeName.SecondOperand)
    {
        return Then(
            expr,
            OptionalDefaultValue(Then(
                Surrounded(op, _whitespaceParser.Whitespace),
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

    private ParseResult<Ast> UnaryExprIndirect(string input, int offset)
    {
        return UnaryExpr(input, offset);
    }

    private ParseResult<Ast> ExprSingle(string input, int offset)
    {
        return WrapInStackTrace(Or(
            FlworExpr,
            IfExpr,
            OrExpr)
        )(input, offset);
    }

    private ParseFunc<Ast> Expr()
    {
        return BinaryOperator(ExprSingle,
            Alias(AstNodeName.SequenceExpr, ","),
            (lhs, rhs) =>
                rhs.Length == 0
                    ? lhs
                    : new Ast(AstNodeName.SequenceExpr, new[] { lhs }.Concat(rhs.Select(x => x.Item2))));
    }

    private ParseFunc<Ast> WrapInStackTrace(ParseFunc<Ast> parser)
    {
        if (!_options.OutputDebugInfo) return parser;

        return (input, offset) =>
        {
            var result = parser(input, offset);

            if (result.IsErr()) return result;

            var start = _stackTraceMap.ContainsKey(offset)
                ? _stackTraceMap[offset]
                : new Ast.StackTraceInfo(offset, -1, -1);

            var end = _stackTraceMap.ContainsKey(result.Offset)
                ? _stackTraceMap[offset]
                : new Ast.StackTraceInfo(result.Offset, -1, -1);

            _stackTraceMap[offset] = start;
            _stackTraceMap[result.Offset] = end;

            return OkWithValue(result.Offset,
                new Ast(AstNodeName.XStackTrace, result.Unwrap())
                {
                    Start = start,
                    End = end
                }
            );
        };
    }

    public static ParseResult<Ast> Parse(string input, ParseOptions options)
    {
        var parser = new XPathParser(options);
        return Complete(Surrounded(parser.Module, parser._whitespaceParser.Whitespace))(input, 0);
    }
}