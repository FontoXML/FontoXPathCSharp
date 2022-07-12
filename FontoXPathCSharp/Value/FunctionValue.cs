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

public class FunctionValue<T> : AbstractValue where T: ISequence
{
    protected readonly ParameterType[] _argumentTypes;
    protected readonly int _arity;
    public readonly bool IsUpdating;
    protected FunctionSignature<T> _value;

    protected FunctionValue(ParameterType[] argumentTypes, int arity, FunctionSignature<T> value,
        ValueType type, bool isUpdating = false) : base(type)
    {
        _argumentTypes = argumentTypes;
        _arity = arity;
        _value = value;
        IsUpdating = isUpdating;
    }

    public FunctionValue(ParameterType[] argumentTypes, int arity, FunctionSignature<T> value,
        bool isUpdating) : this(
        argumentTypes, arity, value, ValueType.Function, isUpdating)
    {
    }

    protected FunctionSignature<T> Value => _value;

    public int GetArity()
    {
        return _arity;
    }
}