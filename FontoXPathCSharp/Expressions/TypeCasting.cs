using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class TypeCasting
{
    public static AtomicValue<TTo> CastToType<TFrom, TTo>(AtomicValue<TFrom> value, ValueType type)
    {
        var result = TryCastToType<TFrom, TTo>(value, type);
        return result switch
        {
            ErrorResult<TTo> errorResult => throw new Exception(errorResult.Message),
            SuccessResult<AtomicValue<TTo>> successResult => successResult.Data,
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
    }

    public static Result<AtomicValue<TTo>> TryCastToType<TFrom, TTo>(AtomicValue<TFrom> value, ValueType type)
    {
        return new ErrorResult<AtomicValue<TTo>>("TryCastToType not implemented yet");
    }
}