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
                // TODO: implement these
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
                // case AstNodeName.InlineFunctionRef:
                // case AstNodeName.MapConstructor:
                // case AstNodeName.ArrayConstructor:
                // case AstNodeName.StringConstructor:
                // case AstNodeName.UnaryLookup:
                return expr;
            default:
                return new Ast(AstNodeName.SequenceExpr, expr);
        }
    }
}