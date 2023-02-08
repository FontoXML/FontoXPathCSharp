using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public enum AstNodeName
{
    NameTest,
    StepExpr,
    XPathAxis,
    Predicate,
    Predicates,
    ArgumentList,
    SequenceExpr,
    PathExpr,
    Arguments,
    FunctionCallExpr,
    FunctionName,
    AttributeTest,
    AnyElementTest,
    PiTest,
    DocumentTest,
    ElementTest,
    CommentTest,
    NamespaceTest,
    AnyKindTest,
    TextTest,
    AnyFunctionTest,
    TypedFunctionTest,
    SchemaAttributeTest,
    AtomicType,
    AnyItemType,
    ParenthesizedItemType,
    TypedMapTest,
    TypedArrayTest,
    Wildcard,
    IntegerConstantExpr,
    FilterExpr,
    Lookup,
    DynamicFunctionInvocationExpr,
    FunctionItem,
    QueryBody,
    FirstOperand,
    SecondOperand,
    OrOp,
    AndOp,
    StringConcatenateOp,
    RangeSequenceExpr,
    AddOp,
    SubtractOp,
    MultiplyOp,
    DivOp,

    // ReSharper disable once InconsistentNaming
    IDivOp,
    ModOp,
    UnionOp,
    IntersectOp,
    ExceptOp,
    InstanceOfExpr,
    ArgExpr,
    SequenceType,
    TreatExpr,
    CastableExpr,
    CastExpr,
    ArrowExpr,
    EqName,
    Operand,
    UnaryMinusOp,
    UnaryPlusOp,
    Value,
    ContextItemExpr,
    StringConstantExpr,
    RootExpr,
    ArgumentPlaceholder,
    All, // *, only used for ast queries such as `FollowPath` or `GetChildren`
    MainModule,
    Prolog,
    VarRef,
    Name,
    DecimalConstantExpr,
    DoubleConstantExpr,
    NotImplemented, // Used for yet unimplemented Ast nodes.
    Module,
    EqualOp,
    NotEqualOp,
    LessThanOrEqualOp,
    LessThanOp,
    GreaterThanOrEqualOp,
    GreaterThanOp,
    IsOp,
    NodeBeforeOp,
    NodeAfterOp,
    EqOp,
    NeOp,
    LtOp,
    LeOp,
    GtOp,
    GeOp,
    PiTarget,
    ElementName,
    TypeName,
    QName,
    Star,
    AttributeName,
    NcName,
    Uri,
    SchemaElementTest,
    SingleType,
    Optional,
    IfThenElseExpr,
    IfClause,
    ThenClause,
    ElseClause,
    StartExpr,
    EndExpr,
    FlworExpr,
    LetClause,
    LetClauseItem,
    TypeDeclaration,
    VoidSequenceType,
    OccurrenceIndicator,
    TypedVariableBinding,
    LetExpr,
    VarName,
    ForClause,
    ForClauseItem,
    AllowingEmpty,
    ForExpr,
    PositionalVariableBinding,
    WhereClause,
    GroupByClause,
    Collation,
    GroupingSpec,
    GroupVarInitialize,
    VarValue,
    OrderByClause,
    Stable,
    OrderBySpec,
    OrderByExpr,
    OrderModifier,
    OrderingKind,
    EmptyOrderingMode,
    ReturnClause,
    XStackTrace,
    WindowClause,
    CountClause,
    InlineFunctionExpr,
    Param,
    Annotation,
    AnnotationName,
    ParamList,
    FunctionBody,
    NamedFunctionRef,
    FunctionTest,
    AnyMapTest,
    AnyArrayTest,
    NamespaceNodeTest
}

public static class AstNodeUtils
{
    public static string GetNodeName(this AstNodeName input)
    {
        var name = Enum.GetName(typeof(AstNodeName), input)!;
        return "xqx:" + char.ToLowerInvariant(name[0]) + name[1..];
    }
}

public class Ast
{
    public readonly AstNodeName Name;
    public readonly Dictionary<string, string?> StringAttributes;
    public List<Ast> Children;
    public string TextContent;

    public Ast(AstNodeName name)
    {
        Name = name;
        StringAttributes = new Dictionary<string, string?>();
        Children = new List<Ast>();
        TextContent = "";
        Start = null;
        End = null;
    }

    public Ast(AstNodeName name, params Ast[] children)
    {
        Name = name;
        StringAttributes = new Dictionary<string, string?>();
        Children = children.ToList();
        TextContent = "";
        Start = null;
        End = null;
    }

    public Ast(AstNodeName name, IEnumerable<Ast> children)
    {
        Name = name;
        StringAttributes = new Dictionary<string, string?>();
        Children = children.ToList();
        TextContent = "";
        Start = null;
        End = null;
    }

    public StackTraceInfo? End { get; set; }
    public StackTraceInfo? Start { get; set; }

    public Ast? GetFirstChild(AstNodeName name = AstNodeName.All)
    {
        return Children.Find(x => name == AstNodeName.All || x.Name == name);
    }

    public Ast? GetFirstChild(params AstNodeName[] names)
    {
        return Children.Find(x => names.Contains(AstNodeName.All) || names.Contains(x.Name));
    }

    public IEnumerable<Ast> GetChildren(AstNodeName name)
    {
        return Children.FindAll(x => name == AstNodeName.All || name == x.Name);
    }

    public Ast? FollowPath(params AstNodeName[] path)
    {
        var ast = this;

        foreach (var p in path)
        {
            ast = ast.GetFirstChild(p);
            if (ast == null) break;
        }

        return ast;
    }

    public QName GetQName()
    {
        return new QName(
            TextContent,
            StringAttributes.ContainsKey("URI") ? StringAttributes["URI"] : null,
            StringAttributes.ContainsKey("prefix") ? StringAttributes["prefix"] : null
        );
    }

    public bool IsA(params AstNodeName[] names)
    {
        return names.Any(x => Name == x || x == AstNodeName.All);
    }

    public override string ToString()
    {
        return IndentedToString(0);
    }

    private string IndentedToString(int indentLevel)
    {
        var indentSize = 2;
        var indent = new string(' ', indentLevel * indentSize);
        var nodeName = Name.GetNodeName();
        var attributes = ' ' + string.Join(' ', StringAttributes.Select(x => $"xqx:{x.Key}=\"{x.Value}\""));
        var opening = $"{indent}<{nodeName}{(attributes.Length > 1 ? attributes : "")}>";
        var closing = $"</{nodeName}>";
        return Children.Count != 0 
            ? $"{opening}" +
              $"{(!string.IsNullOrEmpty(TextContent) ? "\n" + new string(' ', (indentLevel + 1) * indentSize) + TextContent : "")}" +
              $"\n{string.Join("\n", Children.Select(c => c.IndentedToString(indentLevel + 1)))}\n{indent}{closing}"
            : opening + TextContent + closing;
    }

    public Ast AddChildren(IEnumerable<Ast> children)
    {
        Children.AddRange(children);
        return this;
    }

    public Ast AddChild(Ast child)
    {
        Children.Add(child);
        return this;
    }

    public SequenceType GetTypeDeclaration()
    {
        var typeDeclarationAst = GetFirstChild(AstNodeName.TypeDeclaration);
        if (typeDeclarationAst == null || typeDeclarationAst.GetFirstChild(AstNodeName.VoidSequenceType) != null)
            return new SequenceType(ValueType.Item, SequenceMultiplicity.ZeroOrMore);

        Func<Ast, ValueType>? determineType = null;
        determineType = typeAst =>
        {
            return typeAst.Name switch
            {
                AstNodeName.DocumentTest => ValueType.DocumentNode,
                AstNodeName.ElementTest => ValueType.Element,
                AstNodeName.AttributeTest => ValueType.Attribute,
                AstNodeName.PiTest => ValueType.ProcessingInstruction,
                AstNodeName.CommentTest => ValueType.Comment,
                AstNodeName.TextTest => ValueType.Text,
                AstNodeName.AnyKindTest => ValueType.Node,
                AstNodeName.AnyItemType => ValueType.Item,
                AstNodeName.AnyFunctionTest or AstNodeName.FunctionTest or AstNodeName.TypedFunctionTest => ValueType
                    .Function,
                AstNodeName.AnyMapTest or AstNodeName.TypedMapTest => ValueType.Map,
                AstNodeName.AnyArrayTest or AstNodeName.TypedArrayTest => ValueType.Array,
                AstNodeName.AtomicType => string.Join(':',
                        typeAst.StringAttributes.ContainsKey("prefix") ? typeAst.StringAttributes["prefix"] : "",
                        typeAst.TextContent)
                    .StringToValueType(),
                AstNodeName.ParenthesizedItemType => determineType!(typeAst.GetFirstChild()!),
                AstNodeName.SchemaElementTest or AstNodeName.SchemaAttributeTest or AstNodeName.NamespaceNodeTest =>
                    throw new Exception(
                        $"Type declaration '{typeDeclarationAst.GetFirstChild()?.Name}' is not supported."),
                _ => throw new Exception(
                    $"Type declaration '{typeDeclarationAst.GetFirstChild()?.Name}' is not supported.")
            };
        };

        var valueType = determineType(typeDeclarationAst.GetFirstChild()!);

        string? occurrence = null;
        var occurrenceNode = typeDeclarationAst.GetFirstChild(AstNodeName.OccurrenceIndicator);
        if (occurrenceNode != null) occurrence = occurrenceNode.TextContent;

        return occurrence switch
        {
            "*" => new SequenceType(valueType, SequenceMultiplicity.ZeroOrMore),
            "?" => new SequenceType(valueType, SequenceMultiplicity.ZeroOrOne),
            "+" => new SequenceType(valueType, SequenceMultiplicity.OneOrMore),
            _ => new SequenceType(valueType, SequenceMultiplicity.ExactlyOne)
        };
    }

    public class StackTraceInfo
    {
        public int Column;
        public int Line;
        public int Offset;

        public StackTraceInfo(int offset, int line, int column)
        {
            Offset = offset;
            Line = line;
            Column = column;
        }
    }
}