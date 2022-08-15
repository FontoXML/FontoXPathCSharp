using System.Collections;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

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
        return ((IEnumerable<AbstractValue>)_values).GetEnumerator();
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

    public Iterator<AbstractValue> GetValue()
    {
        return IteratorUtils.ArrayIterator(_values);
    }

    public ISequence Filter(Func<AbstractValue, int, ISequence, bool> callback)
    {
        var i = -1;
        return SequenceFactory.CreateFromIterator(_ =>
        {
            i++;
            while (i < _values.Length && !callback(_values[i], i, this)) i++;

            return i >= _values.Length
                ? IteratorResult<AbstractValue>.Done()
                : IteratorResult<AbstractValue>.Ready(_values[i]);
        });
    }

    public ISequence Map(Func<AbstractValue, int, ISequence, AbstractValue> callback)
    {
        var i = -1;
        return SequenceFactory.CreateFromIterator(
            _ => ++i >= _values.Length
                ? IteratorResult<AbstractValue>.Done()
                : IteratorResult<AbstractValue>.Ready(callback(_values[i], i, this)),
            _values.Length
        );
    }

    public ISequence MapAll(Func<AbstractValue[], ISequence> allvalues, IterationHint hint)
    {
        return allvalues(_values);
    }

    public bool GetEffectiveBooleanValue()
    {
        if (_values[0].GetValueType().IsSubtypeOf(ValueType.Node)) return true;
        // We always have a length > 1, or we'd be a singletonSequence
        throw new Exception("FORG0006");
    }
}