using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public delegate T FunctionSignature<out T>(DynamicContext? dynamicContext,
    ExecutionParameters? executionParameters,
    StaticContext? staticContext, params ISequence[] sequences);

public delegate ISequence FunctionDefinitionType(
    DynamicContext? dynamicContext,
    ExecutionParameters? executionParameters,
    StaticContext? staticContext, params ISequence[] sequences);

public class FunctionValue<T> : AbstractValue
{
    protected readonly ParameterType[] _argumentTypes;
    protected readonly int _arity;
    protected readonly bool _isUpdating;
    private string _localName;
    private string _namespaceUri;
    private SequenceType _returnType;
    protected FunctionSignature<T> _value;


    protected FunctionValue(ParameterType[] argumentTypes, int arity, FunctionSignature<T> value,
        ValueType type, bool isUpdating = false) : base(type)
    {
        _argumentTypes = argumentTypes;
        _arity = arity;
        _value = value;
        _isUpdating = isUpdating;
    }

    public FunctionValue(
        ParameterType[] argumentTypes,
        int arity,
        string localName,
        string namespaceUri,
        SequenceType returnType,
        FunctionSignature<T> value,
        bool isAnonymous = false,
        bool isUpdating = false) : base(ValueType.Function)
    {
        _value = value;
        _arity = arity;
        _argumentTypes = ExpandParameterTypeToArity(argumentTypes, arity);
        _isUpdating = isUpdating;
        _localName = localName;
        _namespaceUri = namespaceUri;
        _returnType = returnType;
    }

    public FunctionValue(ParameterType[] argumentTypes, int arity, FunctionSignature<T> value,
        bool isUpdating) : this(
        argumentTypes, arity, value, ValueType.Function, isUpdating)
    {
    }

    public FunctionSignature<T> Value => _value;

    public bool IsUpdating => _isUpdating;

    private ParameterType[]? ExpandParameterTypeToArity(ParameterType[] argumentTypes, int arity)
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

    public int GetArity()
    {
        return _arity;
    }
}