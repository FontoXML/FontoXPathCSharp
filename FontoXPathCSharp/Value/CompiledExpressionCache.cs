using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc = System.Func<LexicalQualifiedName, int, ResolvedQualifiedName?>;

namespace FontoXPathCSharp.Value;

public enum CacheState
{
    Parsed,
    Compiled,
    StaticAnalyzed
}

public struct StaticCompilationResult
{
    public StaticCompilationResult(StaticContext staticContext, AbstractExpression expression)
    {
        StaticContext = staticContext;
        Expression = expression;
    }

    public StaticContext StaticContext { get; }
    public AbstractExpression Expression { get; }
}

public abstract class ExpressionResult
{
    protected CacheState _cacheState;

    public CacheState CacheState => _cacheState;
}

public class ParsedExpressionResult : ExpressionResult
{
    public ParsedExpressionResult(IAST ast)
    {
        _cacheState = CacheState.Parsed;
        AST = ast;
    }

    public IAST AST { get; }
}

public class CachedExpressionResult : ExpressionResult
{
    public CachedExpressionResult(AbstractExpression expression, CacheState cacheState)
    {
        _cacheState = cacheState;
        Expression = expression;
    }

    public AbstractExpression Expression { get; }
}

public class CachedExpression
{
    public CachedExpression(AbstractExpression expression, bool requiresStaticCompilation)
    {
        Expression = expression;
        RequiresStaticCompilation = requiresStaticCompilation;
    }

    public AbstractExpression Expression { get; }

    public bool RequiresStaticCompilation { get; }
}

public class CompiledExpressionCache
{
    public static CachedExpression GetStaticCompilationResultFromCache<TSelector>(
        TSelector xpathSource,
        string language,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, IExternalValue> variables,
        Dictionary<string, string> moduleImports,
        bool compilationOptionsDebug,
        string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver)
    {
        throw new NotImplementedException("Fetching compilation results from cache not supported yet.");
    }
}