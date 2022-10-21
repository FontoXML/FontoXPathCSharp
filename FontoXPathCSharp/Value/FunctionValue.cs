using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public delegate T FunctionSignature<out T, TNode>(
    DynamicContext? dynamicContext,
    ExecutionParameters<TNode> executionParameters,
    StaticContext<TNode>? staticContext, params ISequence[] sequences) where TNode : notnull;

public delegate ISequence FunctionDefinitionType<TNode>(
    DynamicContext? dynamicContext,
    ExecutionParameters<TNode> executionParameters,
    StaticContext<TNode>? staticContext, params ISequence[] sequences) where TNode : notnull;

public class FunctionValue<T, TNode> : AbstractValue where TNode : notnull
{
    private readonly ParameterType[] _argumentTypes;
    public readonly int Arity;
    public readonly bool IsUpdating;
    private string? _localName;
    private string? _namespaceUri;
    private SequenceType? _returnType;


    protected FunctionValue(
        ParameterType[] argumentTypes,
        int arity,
        FunctionSignature<T, TNode> value,
        ValueType type,
        bool isUpdating = false) : base(type)
    {
        _argumentTypes = argumentTypes;
        Arity = arity;
        Value = value;
        IsUpdating = isUpdating;
    }

    public FunctionValue(
        ParameterType[] argumentTypes,
        int arity,
        string localName,
        string namespaceUri,
        SequenceType returnType,
        FunctionSignature<T, TNode> value,
        bool isAnonymous = false,
        bool isUpdating = false) : base(ValueType.Function)
    {
        Value = value;
        Arity = arity;
        _argumentTypes = ExpandParameterTypeToArity(argumentTypes, arity);
        IsUpdating = isUpdating;
        _localName = localName;
        _namespaceUri = namespaceUri;
        _returnType = returnType;
    }

    protected FunctionValue(ParameterType[] argumentTypes, int arity, FunctionSignature<T, TNode> value,
        bool isUpdating) : this(
        argumentTypes, arity, value, ValueType.Function, isUpdating)
    {
    }

    public FunctionSignature<T, TNode> Value { get; init; }

    private static ParameterType[] ExpandParameterTypeToArity(ParameterType[] argumentTypes, int arity)
    {
        var indexOfRest = -1;
        for (var i = 0; i < argumentTypes.Length; i++)
            if (argumentTypes[i] == ParameterType.Ellipsis)
                indexOfRest = i;

        if (indexOfRest > -1)
        {
            var replacePart = Enumerable.Repeat(argumentTypes[indexOfRest - 1], arity - (argumentTypes.Length - 1));
            return argumentTypes.Skip(indexOfRest).Concat(replacePart).ToArray();
        }

        return argumentTypes;
    }
}