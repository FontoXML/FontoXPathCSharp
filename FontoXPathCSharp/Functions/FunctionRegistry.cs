using System.Collections.Concurrent;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Functions;

public static class FunctionRegistry
{
    private static readonly ConcurrentDictionary<string, List<FunctionProperties>> RegisteredFunctionsByName = new();

    public static FunctionProperties? GetFunctionByArity(string functionNamespaceUri, string functionLocalName,
        int arity)
    {
        var index = functionNamespaceUri + ":" + functionLocalName;

        if (!RegisteredFunctionsByName.TryGetValue(index, out var matchingFunctions)) return null;

        var matchingFunction = matchingFunctions.Find(functionDecl =>
        {
            var isEllipsis = Array.Exists(functionDecl.ArgumentTypes, x => x.IsEllipsis);

            if (isEllipsis) return functionDecl.ArgumentTypes.Length - 1 <= arity;

            return functionDecl.ArgumentTypes.Length == arity;
        });

        if (matchingFunction == null) return null;

        return new FunctionProperties(
            matchingFunction.ArgumentTypes,
            arity,
            matchingFunction.CallFunction,
            matchingFunction.IsUpdating,
            functionLocalName,
            functionNamespaceUri,
            matchingFunction.ReturnType
        );
    }

    public static void RegisterFunction(string namespaceUri, string localName, ParameterType[] argumentTypes,
        SequenceType returnType, FunctionSignature<ISequence> callFunction)
    {
        var index = namespaceUri + ":" + localName;

        if (!RegisteredFunctionsByName.ContainsKey(index))
            RegisteredFunctionsByName[index] = new List<FunctionProperties>();

        RegisteredFunctionsByName[index].Add(new FunctionProperties(argumentTypes, argumentTypes.Length,
            callFunction, false, localName, namespaceUri, returnType));
    }
}