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

    protected FunctionValue(ParameterType[] argumentTypes, int arity, FunctionDefinitionType<T> value,
        ValueType type) : base(type)
    {
        _argumentTypes = argumentTypes;
        _arity = arity;
        Value = value;
    }

    public FunctionValue(ParameterType[] argumentTypes, int arity, FunctionDefinitionType<T> value) : this(
        argumentTypes, arity, value, ValueType.Function)
    {
    }

    public int GetArity()
    {
        return _arity;
    }
}