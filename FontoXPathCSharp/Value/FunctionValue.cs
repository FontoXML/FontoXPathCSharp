using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public delegate T FunctionSignature<out T>(DynamicContext? dynamicContext, ExecutionParameters? executionParameters,
    StaticContext? staticContext, params ISequence[] args);

public class FunctionValue<T> : AbstractValue
{
    private readonly ParameterType[] _argumentTypes;
    private readonly int _arity;
    public readonly FunctionSignature<T> Value;

    public FunctionValue(ParameterType[] argumentTypes, int arity, FunctionSignature<T> value) : base(
        ValueType.Function)
    {
        _argumentTypes = argumentTypes;
        _arity = arity;
        Value = value;
    }

    public int GetArity()
    {
        return _arity;
    }
}