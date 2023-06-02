using System.Collections;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Sequences;

public class IteratorBackedSequence : ISequence
{
    private readonly List<AbstractValue> _cachedValues;
    private readonly Iterator<AbstractValue> _value;

    private bool _cacheAllValues;
    private int _currentPosition;
    private int? _length;

    public IteratorBackedSequence(Iterator<AbstractValue> valueIterator, int? predictedLength)
    {
        _cacheAllValues = false;
        _cachedValues = new List<AbstractValue>();
        _currentPosition = 0;
        _length = predictedLength;
        _value = hint =>
        {
            if (_currentPosition >= _length)
                return IteratorResult<AbstractValue>.Done();

            if (_currentPosition < _cachedValues.Count)
                return IteratorResult<AbstractValue>.Ready(_cachedValues[_currentPosition++]);

            var value = valueIterator(hint);
            if (value.IsDone)
            {
                _length = _currentPosition;
                return value;
            }

            if (_cacheAllValues || _currentPosition < 2) _cachedValues.Add(value.Value!);

            _currentPosition++;
            return value;
        };
    }

    public bool IsEmpty()
    {
        if (_length == 0) return true;

        return First() == null;
    }

    public bool IsSingleton()
    {
        if (_length != null) return _length == 1;

        var oldPosition = _currentPosition;

        Reset();
        var value = _value(IterationHint.None);
        if (value.IsDone)
        {
            Reset(oldPosition);
            return false;
        }

        var secondValue = _value(IterationHint.None);
        Reset(oldPosition);
        return secondValue.IsDone;
    }

    public AbstractValue? First()
    {
        if (_cachedValues.Count != 0) return _cachedValues[0];

        var firstValue = _value(IterationHint.None);

        Reset();

        return firstValue.IsDone ? null : firstValue.Value;
    }

    public AbstractValue[] GetAllValues()
    {
        if (_currentPosition > _cachedValues.Count && _length != _cachedValues.Count)
            throw new Exception("Implementation error: Sequence Iterator has progressed.");

        _cacheAllValues = true;

        var val = _value(IterationHint.None);
        while (!val.IsDone) val = _value(IterationHint.None);

        return _cachedValues.ToArray();
    }

    public int GetLength()
    {
        return GetLength(false);
    }

    public Iterator<AbstractValue> GetValue()
    {
        return _value;
    }

    public ISequence Filter(Func<AbstractValue, int, ISequence, bool> callback)
    {
        var i = -1;
        var iterator = _value;

        return SequenceFactory.CreateFromIterator(hint =>
        {
            i++;
            var value = iterator(hint);
            while (!value.IsDone)
            {
                if (value.Value != null && callback(value.Value, i, this)) return value;

                i++;
                value = iterator(IterationHint.None);
            }

            return value;
        });
    }

    public ISequence Map(Func<AbstractValue, int, ISequence, AbstractValue> callback)
    {
        var i = 0;
        var iterator = _value;
        return SequenceFactory.CreateFromIterator(hint =>
        {
            var value = iterator(hint);
            if (value.IsDone) return IteratorResult<AbstractValue>.Done();
            return IteratorResult<AbstractValue>.Ready(callback(value.Value!, i++, this));
        });
    }

    public ISequence MapAll(Func<AbstractValue[], ISequence> callback, IterationHint hint)
    {
        var iterator = _value;
        var allResults = new List<AbstractValue>();

        for (var value = iterator(IterationHint.None);
             !value.IsDone;
             value = iterator(IterationHint.None))
            allResults.Add(value.Value!);

        var mappedResultIterator = callback(allResults.ToArray()).GetValue();
        return SequenceFactory.CreateFromIterator(_ => mappedResultIterator(IterationHint.None));
    }

    public bool GetEffectiveBooleanValue()
    {
        var oldPosition = _currentPosition;

        Reset();
        var it = _value(IterationHint.None);
        if (it.IsDone)
        {
            Reset(oldPosition);
            return false;
        }

        var firstValue = it.Value;
        if (firstValue!.GetValueType().IsSubtypeOf(ValueType.Node))
        {
            Reset(oldPosition);
            return true;
        }

        var secondValue = _value(IterationHint.None);
        if (!secondValue.IsDone)
            throw new XPathException(
                "FORG0006",
                "A wrong argument type was specified in a function call.");

        Reset(oldPosition);
        return firstValue.GetEffectiveBooleanValue();
    }

    public int GetLength(bool onlyIfCheap)
    {
        if (_length.HasValue)
            return (int)_length;

        if (onlyIfCheap) return -1;

        var oldPosition = _currentPosition;

        var length = GetAllValues().Length;

        Reset(oldPosition);

        return length;
    }

    private void Reset(int position = 0)
    {
        _currentPosition = position;
    }

    public override string ToString()
    {
        return "<IteratorBackedSequence>[]";
    }
}