using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class UntypedAtomicValue : AtomicValue
{
    public readonly object Value;

    public UntypedAtomicValue(object value) : base(ValueType.XsUntypedAtomic)
    {
        Value = value;
    }

    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: '" + Value + "']";
    }

    public override object GetValue()
    {
        return Value;
    }
}