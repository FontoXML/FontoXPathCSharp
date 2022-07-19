using FontoXPathCSharp.Expressions;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class IntValue : AtomicValue
{
    public readonly int Value;

    public IntValue(int value) : base(ValueType.XsInt)
    {
        Value = value;
    }

    public IntValue(object? value) : base(ValueType.XsInt)
    {
        Value = value is string s 
            ? int.TryParse(s, out var val) ? val : throw new Exception($"Can't parse {s} into an int.") 
            : (int)(value ?? throw new Exception($"Tried to initialize an IntValue with {value}."));
    }

    public override object GetValue()
    {
        return Value;
    }
}