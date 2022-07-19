using FontoXPathCSharp.Expressions;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class QNameValue : AtomicValue
{
    public QNameValue(QName value) : base(ValueType.XsQName)
    {
        Value = value;
    }

    public QName Value { get; }

    public override QName GetValue()
    {
        return Value;
    }
}