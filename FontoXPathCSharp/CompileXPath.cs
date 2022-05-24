using System.Diagnostics;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc = System.Func<LexicalQualifiedName, int, ResolvedQualifiedName?>;

namespace FontoXPathCSharp;

public static class CompileXPath
{
    public static StaticCompilationResult StaticallyCompileXPath<TSelector>(
        TSelector selector,
        CompilationOptions compilationOptions,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, IExternalValue> variables,
        Dictionary<string, string> moduleImports,
        string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver) where TSelector : notnull
    {
        var specificStaticContext = new ExecutionSpecificStaticContext(namespaceResolver, variables,
            defaultFunctionNamespaceUri, functionNameResolver);
        var rootStaticContext = new StaticContext(specificStaticContext);

        foreach (var modulePrefix in moduleImports.Keys)
        {
            var moduleUri = moduleImports[modulePrefix];
            rootStaticContext.EnhanceWithModule(moduleUri);
            rootStaticContext.RegisterNamespace(modulePrefix, moduleUri);
        }

        if (typeof(TSelector) == typeof(string))
            selector = (TSelector) (object) NormalizeEndOfLines((string) (object) selector);

        var result = CreateExpressionFromSource(
            selector,
            compilationOptions,
            namespaceResolver,
            variables,
            moduleImports,
            defaultFunctionNamespaceUri,
            functionNameResolver
        );

        throw new NotImplementedException("StaticallyCompileXPath not finished yet.");
    }

    private static ExpressionResult CreateExpressionFromSource<TSelector>(
        TSelector xpathSource,
        CompilationOptions compilationOptions,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, IExternalValue> variables,
        Dictionary<string, string> moduleImports,
        string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver)
    {
        var language = compilationOptions.AllowXQuery ? "XQuery" : "XPath";

        var fromCache = compilationOptions.DisableCache
            ? null
            : CompiledExpressionCache.GetStaticCompilationResultFromCache(
                xpathSource,
                language,
                namespaceResolver,
                variables,
                moduleImports,
                compilationOptions.Debug,
                defaultFunctionNamespaceUri,
                functionNameResolver);

        if (fromCache != null)
            return new CachedExpressionResult(fromCache.Expression,
                fromCache.RequiresStaticCompilation ? CacheState.Compiled : CacheState.StaticAnalyzed);

        var ast =
            typeof(TSelector) == typeof(string)
                ? ParseExpression.ParseXPathOrXQueryExpression(xpathSource, compilationOptions)
                : XmlToAst.ConvertXmlToAst(xpathSource);

        return new ParsedExpressionResult(ast);
    }

    private static string NormalizeEndOfLines(string selector)
    {
        Debug.WriteLine("Warning, end of line normalization regex might not be correct yet.");
        return selector.Replace("(\x0D\x0A)|(\x0D(?!\x0A))g", "" + 0xa);
    }
}

public class ResolvedFunction
{
    public ResolvedFunction(LexicalQualifiedName lexicalQualifiedName, int arity,
        ResolvedQualifiedName resolvedQualifiedName)
    {
        Arity = arity;
        LexicalQName = lexicalQualifiedName;
        ResolvedQName = resolvedQualifiedName;
    }

    public int Arity { get; }

    public LexicalQualifiedName LexicalQName { get; }

    public ResolvedQualifiedName ResolvedQName { get; }
}