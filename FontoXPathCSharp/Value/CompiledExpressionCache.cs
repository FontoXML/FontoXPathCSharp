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
    AbstractExpression _expression;
    StaticContext _context;

    public StaticContext StaticContext => _context;
    public AbstractExpression Expression => _expression;
}

public abstract class ExpressionResult
{
    protected CacheState _cacheState;

    public CacheState CacheState => _cacheState;
}

public class ParsedExpressionResult : ExpressionResult
{
    private IAST _ast;

    public ParsedExpressionResult(IAST ast)
    {
        _cacheState = CacheState.Parsed;
        _ast = ast;
    }

    public IAST AST => _ast;
}

public class CachedExpressionResult : ExpressionResult
{
    private AbstractExpression _expression;

    public CachedExpressionResult(AbstractExpression expression, CacheState cacheState)
    {
        _cacheState = cacheState;
        _expression = expression;
    }

    public AbstractExpression Expression => _expression;
}

public class CachedExpression
{
    private AbstractExpression _expression;
    private bool _requiresStaticCompilation;

    public CachedExpression(AbstractExpression expression, bool requiresStaticCompilation)
    {
        _expression = expression;
        _requiresStaticCompilation = requiresStaticCompilation;
    }

    public AbstractExpression Expression => _expression;
    public bool RequiresStaticCompilation => _requiresStaticCompilation;
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