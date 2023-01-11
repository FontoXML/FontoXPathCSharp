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

    // Replaces the x != null ? [x] : [] pattern that's common in the parser.
    public static T[] WrapNullableInArray<T>(T? item)
    {
        return IfNotNullWrapValue(item, item);
    }
    
    // Replaces the x != null ? [y] : [] pattern that's common in the parser
    // often used to wrap an AST if a val is not null.
    public static T2[] IfNotNullWrapValue<T1, T2>(T1? nullable, T2? valToWrap)
    {
        return nullable != null ? new[] { valToWrap! } : Array.Empty<T2>();
    }

    // Convenient when you want to append arrays/lists to singletones.
    public static T[] WrapSingletonInArray<T>(T item)
    {
        return new[] { item };
    }
}