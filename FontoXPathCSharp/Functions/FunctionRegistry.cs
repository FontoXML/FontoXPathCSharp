using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Functions;

public static class FunctionRegistry
{
    private static Dictionary<string, List<FunctionProperties>> _registeredFunctionsByName = new();

    public static FunctionProperties? GetFunctionByArity(string? functionNamespaceUri, string functionLocalName,
        int arity)
    {
        var index = functionNamespaceUri + ":" + functionLocalName;

        if (!_registeredFunctionsByName.TryGetValue(index, out var matchingFunctions)) return null;

        var matchingFunction = matchingFunctions.Find((functionDecl) =>
        {
            var isElipsis = Array.Exists(functionDecl.ArgumentTypes, x => x.IsEllipsis);

            if (isElipsis) return functionDecl.ArgumentTypes.Length - 1 <= arity;

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
        SequenceType returnType, FunctionDefinitionType<ISequence> callFunction)
    {
        var index = namespaceUri + ":" + localName;

        if (!_registeredFunctionsByName.ContainsKey(index))
            _registeredFunctionsByName[index] = new List<FunctionProperties>();

        _registeredFunctionsByName[index].Add(new FunctionProperties(argumentTypes, argumentTypes.Length,
            callFunction, false, localName, namespaceUri, returnType));
    }
}