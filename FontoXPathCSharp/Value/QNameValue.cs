using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class QNameValue : AtomicValue
{
    public QNameValue(QName value) : base(ValueType.XsQName)
    {
        Value = value;
    }

    public QNameValue(object? value) : base(ValueType.XsQName)
    {
        Value = value as QName ?? throw new NotImplementedException($"Haven't implemented Qnames from: {value}");
    }

    public QName Value { get; }

    public override QName GetValue()
    {
        return Value;
    }
}