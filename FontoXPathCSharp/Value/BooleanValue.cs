namespace FontoXPathCSharp.Value;

public class BooleanValue : AbstractValue
{
    public bool Value;

    public BooleanValue(bool value) : base(ValueType.XSBOOLEAN)
    {
        Value = value;
    }
}