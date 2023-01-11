namespace FontoXPathCSharp.Parsing;

public static class ParsingUtils
{
    public static Ast WrapInSequenceExprIfNeeded(Ast expr)
    {
        switch (expr.Name)
        {
            case AstNodeName.VarRef:
            case AstNodeName.ContextItemExpr:
            case AstNodeName.FunctionCallExpr:
            case AstNodeName.SequenceExpr:
            case AstNodeName.DynamicFunctionInvocationExpr:
            // case AstNodeName.ConstantExpr:
            // case AstNodeName.ElementConstructor:
            // case AstNodeName.ComputedElementConstructor:
            // case AstNodeName.ComputedAttributeConstructor:
            // case AstNodeName.ComputedDocumentConstructor:
            // case AstNodeName.ComputedTextConstructor:
            // case AstNodeName.ComputedCommentConstructor:
            // case AstNodeName.ComputedNamespaceConstructor:
            // case AstNodeName.ComputedPIConstructor:
            // case AstNodeName.OrderedExpr:
            // case AstNodeName.UnOrderedExpr:
            // case AstNodeName.NamedFunctionRef:
            case AstNodeName.InlineFunctionExpr:
            // case AstNodeName.MapConstructor:
            // case AstNodeName.ArrayConstructor:
            // case AstNodeName.StringConstructor:
            // case AstNodeName.UnaryLookup:
                return expr;
            default:
                return new Ast(AstNodeName.SequenceExpr, expr);
        }
    }

    public static T[] WrapNullableInArray<T>(T? item)
    {
        return item != null ? new[] { item } : Array.Empty<T>();
    }
}