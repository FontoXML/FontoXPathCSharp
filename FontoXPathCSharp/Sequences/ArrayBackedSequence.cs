using System.Collections;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

internal class ArrayBackedSequence : ISequence
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
        return ((IEnumerable<AbstractValue>) _values).GetEnumerator();
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

    public AbstractValue[] GetAllValues()
    {
        return _values;
    }

    public int GetLength()
    {
        return _values.Length;
    }

    public bool GetEffectiveBooleanValue()
    {
        // if (SubtypeUtils.IsSubtypeOf(this._values[0].type, ValueType.NODE)) {
        //     return true;
        // }
        // We always have a length > 1, or we'd be a singletonSequence
        throw new NotImplementedException();
        //throw errFORG0006();
    }
}