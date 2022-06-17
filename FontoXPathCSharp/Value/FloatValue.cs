using FontoXPathCSharp.Expressions;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class FloatValue : AtomicValue
{
    public readonly float Value;
    
    public FloatValue(float value) : base(ValueType.XsFloat)
    {
        Value = value;
    }
    
    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + Value + "]";
    }
}