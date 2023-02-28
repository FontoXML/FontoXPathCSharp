using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public abstract class NumericValue<TS> : AtomicValue where TS : notnull
{
    public readonly TS Value;

    protected NumericValue(TS value, ValueType type) : base(type)
    {
        if (!type.IsSubtypeOf(ValueType.XsNumeric))
            throw new Exception("Cannot create a NumericValue with a type that does not inherit xs:numeric");
        Value = value;
    }

    protected static TNum NumericCast<TIn, TNum>(TIn val, Func<TIn, TNum> castingFunc)
    {
        try
        {
            return castingFunc(val);
        }
        catch (ArgumentNullException)
        {
            throw new XPathException("FORG0001", $"Tried to create a {typeof(TNum).Name} from null.");
        }
        catch (FormatException)
        {
            throw new XPathException("FORG0001", $"Cannot make a {typeof(TNum).Name} from {val!.ToString()}.");
        }
        catch (OverflowException)
        {
            throw new XPathException("FOCA0001",
                $"Overflow when creating a {typeof(TNum).Name} from {val!.ToString()}.");
        }
    }

    // public override T GetAs<T>()
    // {
    //     if (Type.IsSubtypeOf(ValueType.XsInteger) && typeof(T) == DecimalValue) return (T)(object)new DecimalValue(Value);
    //     return base.GetAs<T>();
    // }

    public override object GetValue()
    {
        return Value;
    }
}