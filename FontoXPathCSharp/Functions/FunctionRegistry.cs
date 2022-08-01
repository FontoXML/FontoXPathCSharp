using System.Collections.Concurrent;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Functions;

public static class FunctionRegistry<TNode>
{
    private static readonly ConcurrentDictionary<string, List<FunctionProperties<TNode>>> RegisteredFunctionsByName =
        new();

    public static FunctionProperties<TNode>? GetFunctionByArity(
        string functionNamespaceUri, string functionLocalName, int arity)
    {
        List<FunctionProperties<TNode>> matchingFunctions;
        if (!RegisteredFunctionsByName.TryGetValue(functionNamespaceUri + ":" + functionLocalName,
                out matchingFunctions)) return null;

        var matchingFunction = matchingFunctions.Find(functionDecl =>
        {
            var isEllipsis = Array.Exists(functionDecl.ArgumentTypes, x => x.IsEllipsis);

            if (isEllipsis) return functionDecl.ArgumentTypes.Length - 1 <= arity;

            return functionDecl.ArgumentTypes.Length == arity;
        });

        if (matchingFunction == null) return null;

        return new FunctionProperties<TNode>(
            matchingFunction.ArgumentTypes,
            arity,
            matchingFunction.CallFunction,
            matchingFunction.IsUpdating,
            functionLocalName,
            functionNamespaceUri,
            matchingFunction.ReturnType
        );
    }

    public static void RegisterFunction(
        string namespaceUri,
        string localName,
        ParameterType[] argumentTypes,
        SequenceType returnType,
        FunctionSignature<ISequence, TNode> callFunction)
    {
        var index = namespaceUri + ":" + localName;

        if (!RegisteredFunctionsByName.ContainsKey(index))
            RegisteredFunctionsByName[index] = new List<FunctionProperties<TNode>>();

        RegisteredFunctionsByName[index].Add(new FunctionProperties<TNode>(argumentTypes, argumentTypes.Length,
            callFunction, false, localName, namespaceUri, returnType));
    }
}