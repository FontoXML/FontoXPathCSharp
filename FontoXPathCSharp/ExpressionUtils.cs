using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Types;

namespace FontoXPathCSharp;

public static class ExpressionUtils<TNode, TSelector> where TSelector : notnull where TNode : notnull
{
    private static readonly Dictionary<TSelector, AbstractExpression<TNode>> PartiallyParsedXPathCache = new();

    private static AbstractExpression<TNode> ParseXPath(TSelector xpathExpression)
    {
        var cachedExpression = CompiledExpressionCache<TSelector, TNode>.Instance
            .GetAnyStaticCompilationResultFromCache(xpathExpression, null, false);

        if (cachedExpression != null) return cachedExpression;

        if (PartiallyParsedXPathCache.ContainsKey(xpathExpression)) return PartiallyParsedXPathCache[xpathExpression];

        var ast = typeof(TSelector) == typeof(string)
            ? ParseExpression.ParseXPathOrXQueryExpression((string)(object)xpathExpression,
                CompilationOptions.XPathMode)
            : XmlToAst.ConvertXmlToAst(xpathExpression);

        var queryBody = ast.FollowPath(AstNodeName.MainModule, AstNodeName.QueryBody, AstNodeName.All);

        if (queryBody == null) throw new Exception("Library modules do not have a specificity");

        var expression =
            CompileAstToExpression<TNode>.CompileAst(queryBody, new CompilationOptions(false, false, false, false));
        PartiallyParsedXPathCache.Add(xpathExpression, expression);

        return expression;
    }

    public static string? GetBucketForSelector(TSelector expression)
    {
        return ParseXPath(expression).GetBucket();
    }

    public static int CompareSpecificity(TSelector expressionA, TSelector expressionB)
    {
        return ParseXPath(expressionA).Specificity.CompareTo(ParseXPath(expressionB).Specificity);
    }
}