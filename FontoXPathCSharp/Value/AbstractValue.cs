using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public abstract class AbstractValue
{
    protected ValueType Type;

    protected AbstractValue(ValueType type)
    {
        Type = type;
    }

    public virtual T GetAs<T>() where T : AbstractValue
    {
        if (this is not T result)
            throw new InvalidCastException($"Casting AbstractValue({Type}) to {typeof(T).Name}");
        return result;
    }

    public ValueType GetValueType()
    {
        return Type;
    }

    public AtomicValue CastToType(ValueType type)
    {
        return GetValueType().IsSubtypeOf(ValueType.XsAnyAtomicType)
            ? TypeCasting.CastToType(GetAs<AtomicValue>(), type)
            : throw new Exception("Can't cast a non-atomic value.");
    }

    public Result<AtomicValue> TryCastToType(ValueType type)
    {
        return GetValueType().IsSubtypeOf(ValueType.XsAnyAtomicType)
            ? TypeCasting.TryCastToType(GetAs<AtomicValue>(), type)
            : new ErrorResult<AtomicValue>("Can't cast a non-atomic value.");
    }
}