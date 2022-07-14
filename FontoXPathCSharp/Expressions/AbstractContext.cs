using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;

namespace FontoXPathCSharp.Expressions;

public abstract class AbstractContext
{
    protected string? registeredDefaultFunctionNamespaceURI;
    protected Dictionary<string, string>? registeredVariableBindingByHashKey;

    protected Dictionary<string, Func<DynamicContext, ExecutionParameters, ISequence>>?
        registeredVariableDeclarationByHashKey;

    public string? RegisteredDefaultFunctionNamespaceUri => registeredDefaultFunctionNamespaceURI;
    public Dictionary<string, string>? RegisteredVariableBindingByHashKey => registeredVariableBindingByHashKey;

    public Dictionary<string, Func<DynamicContext, ExecutionParameters, ISequence>>?
        RegisteredVariableDeclarationByHashKey => registeredVariableDeclarationByHashKey;

    public abstract FunctionProperties? LookupFunction(string? namespaceURI, string localName, int arity,
        bool skipExternal = false);

    public abstract string? LookupVariable(string? namespaceUri, string localName);
    public abstract ResolvedQualifiedName? ResolveFunctionName(LexicalQualifiedName lexicalQName, int arity);
    public abstract string? ResolveNamespace(string prefix, bool useExternalResolver = true);
}