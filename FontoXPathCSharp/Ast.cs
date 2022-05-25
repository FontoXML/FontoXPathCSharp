using FontoXPathCSharp.Value;

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
    All // *
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
    }

    public Ast(AstNodeName name, params Ast[] children)
    {
        Name = name;
        StringAttributes = new Dictionary<string, string?>();
        Children = children.ToList();
        TextContent = "";
    }

    public Ast? GetFirstChild(AstNodeName name = AstNodeName.All)
    {
        return Children.Find(x => name == AstNodeName.All || name == x.Name);
    }

    public Ast? GetFirstChild(AstNodeName[] names)
    {
        return Children.Find(x => names.Contains(AstNodeName.All) || names.Contains(x.Name));
    }

    public IEnumerable<Ast> GetChildren(AstNodeName name)
    {
        return Children.FindAll(x => name == AstNodeName.All || name == x.Name);
    }

    public Ast? FollowPath(IEnumerable<AstNodeName> path)
    {
        var ast = this;

        foreach (var p in path)
        {
            ast = GetFirstChild(p);
            if (ast == null) break;
        }

        return ast;
    }

    public QName GetQName()
    {
        return new QName(TextContent, StringAttributes["URI"],
            StringAttributes["prefix"]);
    }

    public bool IsA(AstNodeName name)
    {
        return Name == name;
    }

    public override string ToString()
    {
        return string.Format("<AST \"{0}\", {{{1}}}, \"{2}\", [{3}]>", Name,
            string.Join(", ", StringAttributes.Select(x => $"{x.Key}: \"{x.Value}\"")),
            TextContent, Children.Count == 0 ? "" : $"\n{string.Join("\n", Children)}\n");
    }
}