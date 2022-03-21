using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Value;

public delegate T FunctionSignature<out T>(DynamicContext? dynamicContext, ExecutionParameters? executionParameters,
    StaticContext? staticContext, params ISequence[] args);

public class FunctionValue<T> : AbstractValue
{
    public readonly FunctionSignature<T> Value;
    private readonly int _arity;

    public FunctionValue(int arity, FunctionSignature<T> value) : base(ValueType.Function)
    {
        _arity = arity;
        Value = value;
    }

    public int GetArity()
    {
        return _arity;
    }
}