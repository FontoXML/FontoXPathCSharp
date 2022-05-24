using System.Collections;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public class ArrayBackedSequence : ISequence, IEnumerable<AbstractValue>
{
    private readonly AbstractValue[] _values;

    public ArrayBackedSequence(AbstractValue[] values)
    {
        _values = values;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<AbstractValue> GetEnumerator()
    {
        return _values.ToList().GetEnumerator();
    }

    public bool IsEmpty()
    {
        return false;
    }

    public bool IsSingleton()
    {
        return false;
    }

    public AbstractValue? First()
    {
        return _values.Length == 0 ? null : _values[0];
    }

    public int GetLength()
    {
        return _values.Length;
    }

    public bool GetEffectiveBooleanValue()
    {
        if (isSubtypeOf(this._values[0].type, ValueType.NODE)) {
            return true;
        }
        // We always have a length > 1, or we'd be a singletonSequence
        throw errFORG0006();
    }

    public override string ToString()
    {
        return "<ArrayBackedSequence>[" + string.Join(", ", _values.Select(value => value.ToString()!).ToArray()) + "]";
    }
}