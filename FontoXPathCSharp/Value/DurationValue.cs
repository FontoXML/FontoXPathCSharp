using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DurationValue : AbstractValue
{
    public DurationValue() : base(ValueType.XsDuration)
    {
    }
}