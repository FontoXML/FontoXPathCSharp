using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.ExpressionResults;

namespace FontoXPathCSharp;

public record StaticCompilationResult<TNode>(StaticContext<TNode> StaticContext, AbstractExpression<TNode> Expression);

public static class CompileXPath<TSelector, TNode> where TSelector : notnull
{
    private static ExpressionResult CreateExpressionFromSource(
        TSelector xpathSource,
        CompilationOptions compilationOptions,
        NamespaceResolver namespaceResolver,
        Dictionary<string, AbstractValue> variables,
        Dictionary<string, string> moduleImports,
        string defaultFunctionNamespaceUri,
        FunctionNameResolver functionNameResolver)
    {
        var language = compilationOptions.AllowXQuery ? "XQuery" : "XPath";

        var fromCache = compilationOptions.DisableCache
            ? null
            : CompiledExpressionCache<TSelector, TNode>.Instance.GetStaticCompilationResultFromCache(
                xpathSource,
                language,
                namespaceResolver,
                variables,
                moduleImports,
                compilationOptions.Debug,
                defaultFunctionNamespaceUri,
                functionNameResolver);

        if (fromCache != null)
            return fromCache.RequiresStaticCompilation
                ? new CompiledExpressionResult<TNode>(fromCache.Expression)
                : new StaticallyAnalyzedExpressionResult<TNode>(fromCache.Expression);

        var ast =
            typeof(TSelector) == typeof(string)
                ? ParseExpression.ParseXPathOrXQueryExpression((string)(object)xpathSource!, compilationOptions)
                : XmlToAst.ConvertXmlToAst(xpathSource);

        return new ParsedExpressionResult(ast);
    }

    public static StaticCompilationResult<TNode> StaticallyCompileXPath(
        TSelector selector,
        CompilationOptions compilationOptions,
        NamespaceResolver namespaceResolver,
        Dictionary<string, AbstractValue> variables,
        Dictionary<string, string> moduleImports,
        string defaultFunctionNamespaceUri,
        FunctionNameResolver functionNameResolver)
    {
        var specificStaticContext = new ExecutionSpecificStaticContext<TNode>(namespaceResolver, variables,
            defaultFunctionNamespaceUri, functionNameResolver);
        var rootStaticContext = new StaticContext<TNode>(specificStaticContext);

        foreach (var modulePrefix in moduleImports.Keys)
        {
            var moduleUri = moduleImports[modulePrefix];
            StaticContext<TNode>.EnhanceWithModule(moduleUri);
            rootStaticContext.RegisterNamespace(modulePrefix, moduleUri);
        }

        if (typeof(TSelector) == typeof(string))
            selector = (TSelector)(object)NormalizeEndOfLines((string)(object)selector);

        var result = CreateExpressionFromSource(
            selector,
            compilationOptions,
            namespaceResolver,
            variables,
            moduleImports,
            defaultFunctionNamespaceUri,
            functionNameResolver
        );


        switch (result.CacheState)
        {
            case CacheState.StaticAnalyzed:
                return new StaticCompilationResult<TNode>(rootStaticContext,
                    ((StaticallyAnalyzedExpressionResult<TNode>)result).Expression);
            case CacheState.Compiled:
            {
                var compiledResult = (CompiledExpressionResult<TNode>)result;
                compiledResult.Expression.PerformStaticEvaluation(rootStaticContext);
                var language = compilationOptions.AllowXQuery ? "XQuery" : "XPath";

                CompiledExpressionCache<TSelector, TNode>.Instance.StoreStaticCompilationResultInCache(
                    selector,
                    language,
                    specificStaticContext,
                    moduleImports,
                    compiledResult.Expression,
                    compilationOptions.Debug,
                    defaultFunctionNamespaceUri
                );
                return new StaticCompilationResult<TNode>(rootStaticContext, compiledResult.Expression);
            }

            case CacheState.Parsed:
            {
                var parsedResult = (ParsedExpressionResult)result;
                var expressionFromAst = BuildExpressionFromAst(
                    parsedResult.Ast,
                    compilationOptions,
                    rootStaticContext
                );
                expressionFromAst.PerformStaticEvaluation(rootStaticContext);

                if (!compilationOptions.DisableCache)
                {
                    var language = compilationOptions.AllowXQuery ? "XQuery" : "XPath";
                    CompiledExpressionCache<TSelector, TNode>.Instance.StoreStaticCompilationResultInCache(
                        selector,
                        language,
                        specificStaticContext,
                        moduleImports,
                        expressionFromAst,
                        compilationOptions.Debug,
                        defaultFunctionNamespaceUri
                    );
                }

                return new StaticCompilationResult<TNode>(rootStaticContext, expressionFromAst);
            }
        }

        throw new NotImplementedException("StaticallyCompileXPath not finished yet.");
    }

    private static AbstractExpression<TNode> BuildExpressionFromAst(
        Ast ast,
        CompilationOptions compilationOptions,
        StaticContext<TNode> rootStaticContext)
    {
        //TODO: AST Annotation
        // AnnotateAst(ast, new AnnotationContext(rootStaticContext));

        var mainModule = ast.GetFirstChild(AstNodeName.MainModule);

        if (mainModule == null) throw new Exception("Can not execute a library module.");

        var queryBodyContents = mainModule.FollowPath(AstNodeName.QueryBody, AstNodeName.All);

        var prolog = mainModule.GetFirstChild(AstNodeName.Prolog);
        if (prolog != null)
            if (!compilationOptions.AllowXQuery)
                throw new Exception(
                    "XPST0003: Use of XQuery functionality is not allowed in XPath context"
                );
        // TODO: Implement prolog processing
        // processProlog(prolog, rootStaticContext);

        return CompileAstToExpression<TNode>.CompileAst(queryBodyContents!, compilationOptions);
    }

    private static string NormalizeEndOfLines(string selector)
    {
        // Console.WriteLine("Warning, end of line normalization regex might not be correct yet.");
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