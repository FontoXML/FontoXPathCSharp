using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;

namespace FontoXPathCSharp.Expressions;

public abstract class AbstractContext<TNode> where TNode : notnull
{
    protected string? RegisteredDefaultFunctionNamespaceUri;

    public List<Dictionary<string, string>>? RegisteredVariableBindingByHashKey { get; protected init; }

    public Dictionary<string, Func<DynamicContext, ExecutionParameters<TNode>, ISequence>>?
        RegisteredVariableDeclarationByHashKey { get; protected init; }

    public abstract FunctionProperties<TNode>? LookupFunction(string? namespaceUri, string localName, int arity,
        bool skipExternal = false);

    public abstract string? LookupVariable(string? namespaceUri, string localName);
    public abstract ResolvedQualifiedName? ResolveFunctionName(LexicalQualifiedName lexicalQName, int arity);
    public abstract string? ResolveNamespace(string? prefix, bool useExternalResolver = true);
}