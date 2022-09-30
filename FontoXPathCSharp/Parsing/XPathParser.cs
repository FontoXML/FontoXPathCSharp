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
    private readonly LiteralParser _literalParser;
    private readonly NameParser _nameParser;
    private readonly TypesParser _typesParser;
    
    private readonly ParseOptions _options;
    private readonly Dictionary<int, ParseResult<Ast>> _pathExprCache;

    private readonly ParseFunc<Ast> AbbrevForwardStep;

    private readonly ParseFunc<Ast> AbbrevReverseStep;

    private readonly ParseFunc<Ast> AbsoluteLocationPath;

    private readonly ParseFunc<Ast> AdditiveExpr;

    private readonly ParseFunc<string> AllowingEmpty;

    private readonly ParseFunc<Ast> AndExpr;

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

    private readonly ParseFunc<Ast> FlworExpr;

    private readonly ParseFunc<Ast> ForBinding;

    private readonly ParseFunc<Ast> ForClause;

    private readonly ParseFunc<Ast> ForwardStep;

    private readonly ParseFunc<Ast> FunctionCall;

    private readonly ParseFunc<Ast> GroupByClause;

    private readonly ParseFunc<Ast> GroupingSpec;

    private readonly ParseFunc<Ast[]> GroupingSpecList;

    private readonly ParseFunc<Ast> GroupingVariable;

    private readonly ParseFunc<Ast> GroupVarInitialize;

    private readonly ParseFunc<Ast> IfExpr;

    private readonly ParseFunc<Ast> InitialClause;

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

    private readonly ParseFunc<Ast> NameTest;

    private readonly ParseFunc<Ast> NodeTest;

    private readonly ParseFunc<string> OccurenceIndicator;

    private readonly ParseFunc<Ast> OrderByClause;

    private readonly ParseFunc<Ast?> OrderModifier;

    private readonly ParseFunc<Ast> OrderSpec;

    private readonly ParseFunc<Ast[]> OrderSpecList;

    private readonly ParseFunc<Ast> OrExpr;


    private readonly ParseFunc<Ast> ParenthesizedExpr;

    private readonly ParseFunc<Ast> PathExpr;

    private readonly ParseFunc<Ast> PiTest;

    private readonly ParseFunc<Ast> PositionalVar;

    private readonly ParseFunc<Ast> PostfixExprWithoutStep;

    private readonly ParseFunc<Ast[]> PostfixExprWithStep;

    private readonly ParseFunc<Ast> Predicate;

    private readonly ParseFunc<Ast?> PredicateList;

    // TODO: add others
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
    private readonly Dictionary<int, Ast.StackTraceInfo> StackTraceMap;

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
    private readonly WhitespaceParser WhitespaceParser;

    private readonly ParseFunc<Ast> Wildcard;

    private XPathParser(ParseOptions options)
    {
        _options = options;

        WhitespaceParser = new WhitespaceParser();
        _nameParser = new NameParser(WhitespaceParser);
        _typesParser = new TypesParser(_nameParser);
        _literalParser = new LiteralParser(WhitespaceParser);

        StackTraceMap = new Dictionary<int, Ast.StackTraceInfo>();
        _pathExprCache = new Dictionary<int, ParseResult<Ast>>();

        Predicate = Delimited(Token("["), Surrounded(Expr(), WhitespaceParser.Whitespace), Token("]"));

        StringLiteral = Map(_options.XQuery
                ? Or(
                    Surrounded(
                        Star(
                            Or(
                                _literalParser.PredefinedEntityRef,
                                _literalParser.CharRef,
                                _literalParser.EscapeQuot,
                                Regex("[^\"&]"))
                        ),
                        Token("\"")
                    ),
                    Surrounded(Star(Or(
                        _literalParser.PredefinedEntityRef,
                        _literalParser.CharRef,
                        _literalParser.EscapeApos,
                        Regex("[^'&]"))), Token("'")))
                : Or(Surrounded(Star(Or(_literalParser.EscapeQuot, Regex("[^\"]"))), Token("\"")),
                    Surrounded(Star(Or(_literalParser.EscapeApos, Regex("[^']"))), Token("'"))),
            x => string.Join("", x)
        );

        ElementTest = Or(
            Map(
                PrecededMultiple(new[] { Token("element"), WhitespaceParser.Whitespace },
                    Delimited(
                        Followed(Token("("), WhitespaceParser.Whitespace),
                        Then(
                            _nameParser.ElementNameOrWildcard,
                            PrecededMultiple(
                                new[] { WhitespaceParser.Whitespace, Token(","), WhitespaceParser.Whitespace },
                                _typesParser.TypeName),
                            (elemName, typeName) => (
                                nameOrWildcard: new Ast(AstNodeName.ElementName, elemName),
                                type: typeName.GetAst(AstNodeName.TypeName)
                            )
                        ),
                        Preceded(WhitespaceParser.Whitespace, Token(")"))
                    )
                ),
                x => new Ast(AstNodeName.ElementTest, x.nameOrWildcard, x.type)
            ),
            Map(
                PrecededMultiple(new[] { Token("element"), WhitespaceParser.Whitespace },
                    Delimited(
                        Token("("), _nameParser.ElementNameOrWildcard, Token(")")
                    )
                ),
                nameOrWildcard => new Ast(AstNodeName.ElementTest, new Ast(AstNodeName.ElementName, nameOrWildcard))
            ),
            Map(
                PrecededMultiple(new[] { Token("element"), WhitespaceParser.Whitespace },
                    Delimited(
                        Token("("), WhitespaceParser.Whitespace, Token(")")
                    )
                ),
                _ => new Ast(AstNodeName.ElementTest)
            )
        );

        AttributeTest = Or(
            Map(
                PrecededMultiple(new[] { Token("attribute"), WhitespaceParser.Whitespace },
                    Delimited(
                        Followed(Token("("), WhitespaceParser.Whitespace),
                        Then(
                            _nameParser.AttributeNameOrWildcard,
                            PrecededMultiple(
                                new[] { WhitespaceParser.Whitespace, Token(","), WhitespaceParser.Whitespace },
                                _typesParser.TypeName),
                            (attrName, typeName) => (
                                nameOrWildcard: new Ast(AstNodeName.AttributeName, attrName),
                                type: typeName.GetAst(AstNodeName.TypeName)
                            )
                        ),
                        Preceded(WhitespaceParser.Whitespace, Token(")"))
                    )
                ),
                x => new Ast(AstNodeName.AttributeTest, x.nameOrWildcard, x.type)
            ),
            Map(
                PrecededMultiple(new[] { Token("attribute"), WhitespaceParser.Whitespace },
                    Delimited(
                        Token("("), _nameParser.AttributeNameOrWildcard, Token(")")
                    )
                ),
                nameOrWildcard => new Ast(AstNodeName.AttributeTest, new Ast(AstNodeName.AttributeName, nameOrWildcard))
            ),
            Map(
                PrecededMultiple(new[] { Token("attribute"), WhitespaceParser.Whitespace },
                    Delimited(
                        Token("("), WhitespaceParser.Whitespace, Token(")")
                    )
                ),
                _ => new Ast(AstNodeName.AttributeTest)
            )
        );

        ElementDeclaration = _nameParser.ElementName;

        SchemaElementTest = Map(
            Delimited(
                Token("schema-element("),
                Surrounded(ElementDeclaration, WhitespaceParser.Whitespace),
                Token(")")
            ),
            x => x.GetAst(AstNodeName.SchemaElementTest)
        );

        AttributeName = _nameParser.EqName;

        AttributeDeclaration = AttributeName;

        SchemaAttributeTest = Map(
            Delimited(
                Token("schema-attribute("),
                Surrounded(AttributeDeclaration, WhitespaceParser.Whitespace),
                Token(")")
            ),
            decl => decl.GetAst(AstNodeName.SchemaAttributeTest)
        );

        PiTest = Or(
            Map(
                Delimited(
                    Token("processing-instruction("),
                    Surrounded(Or(_nameParser.NcName, StringLiteral), WhitespaceParser.Whitespace),
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
                Surrounded(Optional(Or(ElementTest, SchemaElementTest)), WhitespaceParser.Whitespace),
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
            _literalParser.CommentTest,
            _literalParser.TextTest,
            _literalParser.NamespaceNodeTest,
            _literalParser.AnyKindTest
        );

        Wildcard = Or(
            Map(Preceded(Token("*:"), _nameParser.NcName),
                x => new Ast(AstNodeName.Wildcard, new Ast(AstNodeName.Star),
                    new Ast(AstNodeName.NcName) { TextContent = x })
            ),
            Alias(new Ast(AstNodeName.Wildcard), "*"),
            Map(Followed(_nameParser.BracedUriLiteral, Token("*")),
                x => new Ast(AstNodeName.Wildcard, new Ast(AstNodeName.Uri) { TextContent = x },
                    new Ast(AstNodeName.Star))),
            Map(Followed(_nameParser.NcName, Token(":*")), x =>
                new Ast(AstNodeName.Wildcard, new Ast(AstNodeName.NcName) { TextContent = x },
                    new Ast(AstNodeName.Star))
            )
        );

        NameTest = Or(
            Wildcard,
            Map(_nameParser.EqName, x => new Ast(AstNodeName.NameTest)
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

        ForwardStep = Or(Then(_literalParser.ForwardAxis, NodeTest,
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
            Then(_literalParser.ReverseAxis, NodeTest,
                (axis, test) =>
                    new Ast(AstNodeName.StepExpr,
                        new Ast(AstNodeName.XPathAxis) { TextContent = axis },
                        test
                    )),
            AbbrevReverseStep
        );

        PredicateList = Map(Star(Preceded(WhitespaceParser.Whitespace, Predicate)),
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

        Literal = Or(_literalParser.NumericLiteral, Map(StringLiteral, x => new Ast(AstNodeName.StringConstantExpr,
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
                            Star(Preceded(Surrounded(Token(","), WhitespaceParser.Whitespace), Argument)),
                            (first, following) => following.Prepend(first).ToArray())
                    ),
                    WhitespaceParser.Whitespace
                ),
                Token(")")
            ),
            x => x ?? Array.Empty<Ast>()
        );

        FunctionCall = Preceded(
            Not(
                FollowedMultiple(_literalParser.ReservedFunctionNames,
                    new[] { WhitespaceParser.Whitespace, Token("(") }),
                new[] { "cannot use reserved keyword for function names" }),
            Then(_nameParser.EqName, Preceded(WhitespaceParser.Whitespace, ArgumentList),
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

        AtomicOrUnionType = Map(_nameParser.EqName, x => x.GetAst(AstNodeName.AtomicType));

        ItemType = Or(KindTest, AtomicOrUnionType);

        OccurenceIndicator = Or(Token("?"), Token("*"), Token("+"));

        SequenceType = Or(
            Map(Token("empty-sequence()"), _ => new[] { new Ast(AstNodeName.VoidSequenceType) }),
            Then(
                ItemTypeIndirect,
                Optional(Preceded(WhitespaceParser.Whitespace, OccurenceIndicator)),
                (type, occurrence) =>
                    new[] { type }
                        .Concat(occurrence != null
                            ? new[] { new Ast(AstNodeName.OccurrenceIndicator) { TextContent = occurrence } }
                            : Array.Empty<Ast>())
                        .ToArray())
        );

        TypeDeclaration = Map(
            PrecededMultiple(new[] { Token("as"), WhitespaceParser.WhitespacePlus }, SequenceType),
            x => new Ast(AstNodeName.TypeDeclaration, x)
        );

        VarName = _nameParser.EqName;

        LetBinding = Then3(
            Preceded(Token("$"), VarName),
            Preceded(WhitespaceParser.Whitespace, Optional(TypeDeclaration)),
            Preceded(Surrounded(Token(":="), WhitespaceParser.Whitespace), ExprSingle),
            (variableName, typeDecl, letExpr) =>
            {
                return new Ast(AstNodeName.LetClauseItem,
                    new Ast(AstNodeName.TypedVariableBinding,
                        new[] { variableName.GetAst(AstNodeName.VarName) }
                            .Concat(typeDecl != null
                                ? new[] { typeDecl }
                                : Array.Empty<Ast>())),
                    new Ast(AstNodeName.LetExpr, letExpr));
            }
        );

        AllowingEmpty = Delimited(Token("allowing"), WhitespaceParser.WhitespacePlus, Token("empty"));

        PositionalVar = Map(
            PrecededMultiple(new[] { Token("at"), WhitespaceParser.WhitespacePlus, Token("$") }, VarName),
            x => x.GetAst(AstNodeName.PositionalVariableBinding)
        );

        ForBinding = Then5(
            Preceded(Token("$"), VarName),
            Preceded(WhitespaceParser.Whitespace, Optional(TypeDeclaration)),
            Preceded(WhitespaceParser.Whitespace, Optional(AllowingEmpty)),
            Preceded(WhitespaceParser.Whitespace, Optional(PositionalVar)),
            Preceded(Surrounded(Token("in"), WhitespaceParser.Whitespace), ExprSingle),
            (variableName, typeDecl, empty, pos, forExpr) =>
                new Ast(AstNodeName.ForClauseItem,
                    new[]
                        {
                            new Ast(AstNodeName.TypedVariableBinding,
                                variableName
                                    .GetAst(AstNodeName.VarName)
                                    .AddChildren(typeDecl != null ? new[] { typeDecl } : Array.Empty<Ast>())
                            )
                        }
                        .Concat(empty != null ? new[] { new Ast(AstNodeName.AllowingEmpty) } : Array.Empty<Ast>())
                        .Concat(pos != null ? new[] { pos } : Array.Empty<Ast>())
                        .Concat(new[] { new Ast(AstNodeName.ForExpr, forExpr) })
                )
        );

        ForClause = PrecededMultiple(
            new[] { Token("for"), WhitespaceParser.WhitespacePlus },
            BinaryOperator(
                ForBinding,
                Alias(AstNodeName.SequenceExpr, ","),
                (lhs, rhs) =>
                    new Ast(AstNodeName.ForClause, new[] { lhs }.Concat(rhs.Select(x => x.Item2)))));

        LetClause = Map(
            PrecededMultiple(
                new[] { Token("let"), WhitespaceParser.Whitespace },
                BinaryOperator(
                    LetBinding,
                    Alias(AstNodeName.Arguments, ","),
                    (lhs, rhs) => new[] { lhs }.Concat(rhs.Select(e => e.Item2)))),
            x => new Ast(AstNodeName.LetClause, x)
        );

        InitialClause = Or(ForClause, LetClause);

        WhereClause = Map(
            PrecededMultiple(
                new[] { Token("where"), _literalParser.AssertAdjacentOpeningTerminal, WhitespaceParser.Whitespace },
                ExprSingle),
            x => new Ast(AstNodeName.WhereClause, x)
        );

        UriLiteral = StringLiteral;

        GroupingVariable = Map(
            Preceded(Token("$"), VarName),
            x => x.GetAst(AstNodeName.VarName)
        );

        GroupVarInitialize = Then(
            Preceded(WhitespaceParser.Whitespace, Optional(TypeDeclaration)),
            Preceded(Surrounded(Token(":="), WhitespaceParser.Whitespace), ExprSingle),
            (t, val) => new Ast(
                AstNodeName.GroupVarInitialize,
                (t != null ? new[] { new Ast(AstNodeName.TypeDeclaration, t) } : Array.Empty<Ast>())
                .Concat(new[] { new Ast(AstNodeName.VarValue, val) }))
        );

        GroupingSpec = Then3(
            GroupingVariable,
            Optional(GroupVarInitialize),
            Optional(Map(Preceded(Surrounded(Token("collation"), WhitespaceParser.Whitespace), UriLiteral),
                x => new Ast(AstNodeName.Collation) { TextContent = x })
            ),
            (variableName, init, col) => new Ast(
                AstNodeName.GroupingSpec,
                new[] { variableName }
                    .Concat(init != null ? new[] { init } : Array.Empty<Ast>())
                    .Concat(col != null ? new[] { col } : Array.Empty<Ast>())));

        GroupingSpecList = BinaryOperator(
            GroupingSpec,
            Alias(AstNodeName.Arguments, ","),
            (lhs, rhs) => new[] { lhs }.Concat(rhs.Select(x => x.Item2)).ToArray()
        );

        GroupByClause = Map(
            PrecededMultiple(
                new[] { Token("group"), WhitespaceParser.WhitespacePlus, Token("by"), WhitespaceParser.Whitespace },
                GroupingSpecList),
            x => new Ast(AstNodeName.GroupByClause, x)
        );

        OrderModifier = Then3(
            Optional(Or(Token("ascending"), Token("descending"))),
            Optional(PrecededMultiple(
                new[] { WhitespaceParser.Whitespace, Token("empty"), WhitespaceParser.Whitespace },
                Or(new[] { Token("greatest"), Token("least") }.Select(x => Map(x, y => "empty " + y)).ToArray()))),
            Preceded(WhitespaceParser.Whitespace,
                Optional(PrecededMultiple(new[] { Token("collation"), WhitespaceParser.Whitespace }, UriLiteral))),
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
            Preceded(WhitespaceParser.Whitespace, OrderModifier),
            (orderByExpr, modifier) =>
                new Ast(AstNodeName.OrderBySpec,
                    new[] { new Ast(AstNodeName.OrderByExpr, orderByExpr) }
                        .Concat(modifier != null ? new[] { modifier } : Array.Empty<Ast>()))
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
                    PrecededMultiple(new[] { Token("order"), WhitespaceParser.WhitespacePlus }, Token("by")),
                    _ => false),
                Map(
                    PrecededMultiple(
                        new[]
                        {
                            Token("stable"), WhitespaceParser.WhitespacePlus, Token("order"),
                            WhitespaceParser.WhitespacePlus
                        },
                        Token("by")),
                    _ => true)
            ),
            Preceded(WhitespaceParser.Whitespace, OrderSpecList),
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
            PrecededMultiple(new[] { Token("return"), WhitespaceParser.Whitespace }, ExprSingle),
            x => new Ast(AstNodeName.ReturnClause, x)
        );

        FlworExpr = Then3(
            InitialClause,
            Star(Preceded(WhitespaceParser.Whitespace, IntermediateClause)),
            Preceded(WhitespaceParser.Whitespace, ReturnClause),
            (initial, intermediate, ret) => new Ast(AstNodeName.FlworExpr,
                new[] { initial }.Concat(intermediate).Concat(new[] { ret }))
        );

        ParenthesizedExpr = Or(
            Delimited(Token("("), Surrounded(Expr(), WhitespaceParser.Whitespace), Token(")")),
            Map(Delimited(Token("("), WhitespaceParser.Whitespace, Token(")")), _ => new Ast(AstNodeName.SequenceExpr))
        );

        PrimaryExpr = Or(
            Literal,
            _nameParser.VarRef,
            ParenthesizedExpr,
            _literalParser.ContextItemExpr,
            FunctionCall
        );

        PostfixExprWithStep = Then(
            Map(PrimaryExpr, ParsingUtils.WrapInSequenceExprIfNeeded),
            Star(
                Or(
                    Map(Preceded(WhitespaceParser.Whitespace, Predicate),
                        x => new Ast(AstNodeName.Predicate, x)),
                    Map(Preceded(WhitespaceParser.Whitespace, ArgumentList),
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
                    Preceded(WhitespaceParser.Whitespace,
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
                Preceded(WhitespaceParser.Whitespace, _literalParser.LocationPathAbbreviation),
                Preceded(WhitespaceParser.Whitespace, RelativePathExprWithForcedStepIndirect),
                (lhs, abbrev, rhs) => new Ast(AstNodeName.PathExpr, new[] { lhs, abbrev }.Concat(rhs).ToArray())),
            Then(
                StepExprWithForcedStep,
                Preceded(Surrounded(Token("/"), WhitespaceParser.Whitespace), RelativePathExprWithForcedStepIndirect),
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
                Preceded(WhitespaceParser.Whitespace, _literalParser.LocationPathAbbreviation),
                Preceded(WhitespaceParser.Whitespace, RelativePathExprWithForcedStepIndirect),
                (lhs, abbrev, rhs) => new[] { lhs, abbrev }.Concat(rhs).ToArray()
            ),
            Then(
                StepExprWithForcedStep,
                Preceded(Surrounded(Token("/"), WhitespaceParser.Whitespace), RelativePathExprWithForcedStepIndirect),
                (lhs, rhs) => new[] { lhs }.Concat(rhs).ToArray()), Map(StepExprWithForcedStep, x => new[] { x }),
            Map(StepExprWithForcedStep, x => new[] { x })
        );

        AbsoluteLocationPath = Or(Map(
                PrecededMultiple(new[] { Token("/"), WhitespaceParser.Whitespace }, RelativePathExprWithForcedStep),
                path => new Ast(AstNodeName.PathExpr, new[] { new Ast(AstNodeName.RootExpr) }.Concat(path).ToArray())),
            Then(_literalParser.LocationPathAbbreviation,
                Preceded(WhitespaceParser.Whitespace, RelativePathExprWithForcedStep),
                (abbrev, path) => new Ast(AstNodeName.PathExpr,
                    new[] { new Ast(AstNodeName.RootExpr), abbrev }.Concat(path).ToArray())),
            Map(Followed(Token("/"), Not(Preceded(WhitespaceParser.Whitespace, Regex("[*a-zA-Z]")),
                    new[]
                    {
                        "Single rootExpr cannot be followed by something that can be interpreted as a relative path"
                    })),
                _ => new Ast(AstNodeName.PathExpr, new Ast(AstNodeName.RootExpr)))
        );

        PathExpr = Cached(
            Or(RelativePathExpr, AbsoluteLocationPath),
            _pathExprCache
        );

        ValueExpr = Or(
            // TODO: ValidateExpr(),
            // TODO: ExtensionExpr(),
            // TODO: SimpleMapExpr(),
            PathExpr
        );

        UnaryExpr = Or(
            Then(
                Or(
                    Alias(AstNodeName.UnaryMinusOp, "-"),
                    Alias(AstNodeName.UnaryPlusOp, "+")
                ),
                Preceded(WhitespaceParser.Whitespace, UnaryExprIndirect),
                (op, value) => new Ast(op, new Ast(AstNodeName.Operand, value))
            ),
            ValueExpr
        );

        ArrowFunctionSpecifier = Or(Map(_nameParser.EqName, x => x.GetAst(AstNodeName.EqName)),
            _nameParser.VarRef,
            ParenthesizedExpr
        );

        ArrowExpr = Then(
            UnaryExpr,
            Star(
                PrecededMultiple(
                    new[]
                    {
                        WhitespaceParser.Whitespace, Token("=>"), WhitespaceParser.Whitespace
                    },
                    Then(
                        ArrowFunctionSpecifier,
                        Preceded(WhitespaceParser.Whitespace, ArgumentList),
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
                        WhitespaceParser.Whitespace,
                        Token("cast"),
                        WhitespaceParser.WhitespacePlus,
                        Token("as"),
                        _literalParser.AssertAdjacentOpeningTerminal,
                        WhitespaceParser.Whitespace
                    },
                    _typesParser.SingleType)
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
                        WhitespaceParser.Whitespace,
                        Token("castable"),
                        WhitespaceParser.WhitespacePlus,
                        Token("as"),
                        _literalParser.AssertAdjacentOpeningTerminal,
                        WhitespaceParser.Whitespace
                    },
                    _typesParser.SingleType)
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
                        WhitespaceParser.Whitespace,
                        Token("treat"),
                        WhitespaceParser.WhitespacePlus,
                        Token("as"),
                        _literalParser.AssertAdjacentOpeningTerminal,
                        WhitespaceParser.Whitespace
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
                        WhitespaceParser.Whitespace,
                        Token("instance"),
                        WhitespaceParser.WhitespacePlus,
                        Token("of"),
                        _literalParser.AssertAdjacentOpeningTerminal,
                        WhitespaceParser.Whitespace
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
                    WhitespaceParser.Whitespace,
                    Token("("),
                    WhitespaceParser.Whitespace
                }, Expr()),
                PrecededMultiple(new[]
                {
                    WhitespaceParser.Whitespace,
                    Token(")"),
                    WhitespaceParser.Whitespace,
                    Token("then"),
                    _literalParser.AssertAdjacentOpeningTerminal,
                    WhitespaceParser.Whitespace
                }, ExprSingle),
                (ifClause, thenClause) => new[] { ifClause, thenClause }
            ),
            PrecededMultiple(new[]
            {
                WhitespaceParser.Whitespace,
                Token("else"),
                _literalParser.AssertAdjacentOpeningTerminal,
                WhitespaceParser.Whitespace
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
                _literalParser.AssertAdjacentOpeningTerminal
            ),
            DefaultBinaryOperatorFn
        );

        UnionExpr = BinaryOperator(IntersectExpr,
            Or(
                Alias(AstNodeName.UnionOp, "|"),
                Followed(Alias(AstNodeName.UnionOp, "union"), _literalParser.AssertAdjacentOpeningTerminal)
            ),
            DefaultBinaryOperatorFn
        );

        MultiplicativeExpr = BinaryOperator(
            UnionExpr,
            Or(
                Alias(AstNodeName.MultiplyOp, "*"),
                Followed(Alias(AstNodeName.DivOp, "div"), _literalParser.AssertAdjacentOpeningTerminal),
                Followed(Alias(AstNodeName.IDivOp, "idiv"), _literalParser.AssertAdjacentOpeningTerminal),
                Followed(Alias(AstNodeName.ModOp, "mod"), _literalParser.AssertAdjacentOpeningTerminal)
            ),
            DefaultBinaryOperatorFn
        );

        AdditiveExpr = BinaryOperator(MultiplicativeExpr, Or(
            Alias(AstNodeName.SubtractOp, "-"),
            Alias(AstNodeName.AddOp, "+")
        ), DefaultBinaryOperatorFn);

        RangeExpr = NonRepeatableBinaryOperator(AdditiveExpr,
            Followed(Alias(AstNodeName.RangeSequenceExpr, "to"), _literalParser.AssertAdjacentOpeningTerminal),
            AstNodeName.StartExpr,
            AstNodeName.EndExpr);

        StringConcatExpr =
            BinaryOperator(RangeExpr, Alias(AstNodeName.StringConcatenateOp, "||"), DefaultBinaryOperatorFn);

        ComparisonExpr = NonRepeatableBinaryOperator(StringConcatExpr, Or(
            _literalParser.ValueCompare,
            _literalParser.NodeCompare,
            _literalParser.GeneralCompare
        ));

        AndExpr = BinaryOperator(ComparisonExpr,
            Followed(Alias(AstNodeName.AndOp, "and"), _literalParser.AssertAdjacentOpeningTerminal),
            DefaultBinaryOperatorFn);

        OrExpr = BinaryOperator(AndExpr,
            Followed(Alias(AstNodeName.OrOp, "or"), _literalParser.AssertAdjacentOpeningTerminal),
            DefaultBinaryOperatorFn);

        QueryBody = Map(Expr(), x => new Ast(AstNodeName.QueryBody, x));

        Prolog = NotImplementedAst();

        VersionDeclaration = NotImplementedAst();

        LibraryModule = NotImplementedAst();

        MainModule = Then(Optional(Prolog),
            Preceded(WhitespaceParser.Whitespace, QueryBody),
            (prologPart, body) => new Ast(AstNodeName.MainModule, body)
        );

        Module = Then(
            Optional(Surrounded(VersionDeclaration, WhitespaceParser.Whitespace)),
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
            Star(Then(Surrounded(op, WhitespaceParser.Whitespace), expr, (a, b) => (a, b))),
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
                Surrounded(op, WhitespaceParser.Whitespace),
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
        return BinaryOperator(ExprSingle, Alias(AstNodeName.SequenceExpr, ","), (lhs, rhs) =>
            rhs.Length == 0
                ? lhs
                : new Ast(AstNodeName.SequenceExpr, rhs.Select(x => x.Item2).ToArray()));
    }

    private ParseFunc<Ast> WrapInStackTrace(ParseFunc<Ast> parser)
    {
        if (!_options.OutputDebugInfo) return parser;

        return (input, offset) =>
        {
            var result = parser(input, offset);

            if (result.IsErr()) return result;

            var start = StackTraceMap.ContainsKey(offset)
                ? StackTraceMap[offset]
                : new Ast.StackTraceInfo(offset, -1, -1);

            var end = StackTraceMap.ContainsKey(result.Offset)
                ? StackTraceMap[offset]
                : new Ast.StackTraceInfo(result.Offset, -1, -1);

            StackTraceMap[offset] = start;
            StackTraceMap[result.Offset] = end;

            return OkWithValue(result.Offset,
                new Ast(AstNodeName.XStackTrace, result.Unwrap())
                {
                    _start = start,
                    _end = end
                }
            );
        };
    }

    public static ParseResult<Ast> Parse(string input, ParseOptions options)
    {
        var parser = new XPathParser(options);
        return Complete(Surrounded(parser.Module, parser.WhitespaceParser.Whitespace))(input, 0);
    }
}