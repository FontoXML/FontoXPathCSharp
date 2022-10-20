using FontoXPathCSharp.Expressions;

namespace FontoXPathCSharp.Value;

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

public class CompiledExpressionResult<TNode> : ExpressionResult where TNode : notnull
{
    public CompiledExpressionResult(AbstractExpression<TNode> expression)
    {
        CacheState = CacheState.Compiled;
        Expression = expression;
    }

    public AbstractExpression<TNode> Expression { get; }
}

public class StaticallyAnalyzedExpressionResult<TNode> : ExpressionResult where TNode : notnull
{
    public StaticallyAnalyzedExpressionResult(AbstractExpression<TNode> expression)
    {
        CacheState = CacheState.StaticAnalyzed;
        Expression = expression;
    }

    public AbstractExpression<TNode> Expression { get; }
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