using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public delegate T FunctionDefinitionType<out T>(DynamicContext? dynamicContext,
    ExecutionParameters? executionParameters,
    StaticContext? staticContext, params ISequence[] sequences);

public class FunctionValue<T> : AbstractValue
{
    private readonly ParameterType[] _argumentTypes;
    private readonly int _arity;
    public readonly FunctionDefinitionType<T> Value;
    public readonly bool IsUpdating;

    protected FunctionValue(ParameterType[] argumentTypes, int arity, FunctionDefinitionType<T> value,
        ValueType type, bool isUpdating = false) : base(type)
    {
        _argumentTypes = argumentTypes;
        _arity = arity;
        Value = value;
        IsUpdating = isUpdating;
    }

    public FunctionValue(ParameterType[] argumentTypes, int arity, FunctionDefinitionType<T> value, bool isUpdating) : this(
        argumentTypes, arity, value, ValueType.Function, isUpdating)
    {
    }

    public int GetArity()
    {
        return _arity;
    }
}