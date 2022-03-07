namespace FontoXPathCSharp.Value;

public class BooleanValue : AbstractValue
{
    private readonly bool Value;

    public BooleanValue(bool value) : base(ValueType.XsBoolean)
    {
        Value = value;
    }

    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + Value + "]";
    }
}