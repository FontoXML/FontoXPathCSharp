using FontoXPathCSharp.Expressions;

namespace FontoXPathCSharp.Value.ExpressionResults;

public enum CacheState
{
    Parsed,
    Compiled,
    StaticAnalyzed
}

public abstract class ExpressionResult
{
    public CacheState CacheState { get; protected init; }
}

public class CompiledExpressionResult : ExpressionResult
{
    public CompiledExpressionResult(AbstractExpression expression)
    {
        CacheState = CacheState.Compiled;
        Expression = expression;
    }

    public AbstractExpression Expression { get; }
}

public class StaticallyAnalyzedExpressionResult : ExpressionResult
{
    public StaticallyAnalyzedExpressionResult(AbstractExpression expression)
    {
        CacheState = CacheState.StaticAnalyzed;
        Expression = expression;
    }

    public AbstractExpression Expression { get; }
}

public class ParsedExpressionResult : ExpressionResult
{
    public ParsedExpressionResult(Ast ast)
    {
        CacheState = CacheState.Parsed;
        Ast = ast;
    }

    public Ast Ast { get; }
}