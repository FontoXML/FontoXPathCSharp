using System.Collections;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public class IteratorBackedSequence : ISequence
{
    private readonly Iterator<AbstractValue> _value;

    private bool _cacheAllValues;
    private AbstractValue[] _cachedValues;
    private int _currentPosition;
    private int? _length;

    public IteratorBackedSequence(Iterator<AbstractValue> valueIterator, int? predictedLength)
    {
        _value = valueIterator;

        _cacheAllValues = false;
        _cachedValues = Array.Empty<AbstractValue>();
        _currentPosition = 0;
        _length = predictedLength;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<AbstractValue> GetEnumerator()
    {
        throw new NotImplementedException("GetEnumerator for IteratorBackedSequence");
    }

    private IteratorResult<AbstractValue> Next(IterationHint hint)
    {
        if (_length != null && this._currentPosition >= _length)
        {
            return IteratorResult<AbstractValue>.Done();
        }

        var value = _value(hint);
        if (value.IsDone)
        {
            _length = _currentPosition;
            return value;
        }

        _currentPosition++;
        return value;
    }

    public bool IsEmpty()
    {
        if (_length == 0)
        {
            return true;
        }

        return First() == null;
    }

    public bool IsSingleton()
    {
        if (_length != null)
        {
            return _length == 1;
        }

        var oldPosition = _currentPosition;

        Reset();
        var value = Next(IterationHint.None);
        if (value.IsDone)
        {
            Reset(oldPosition);
            return false;
        }

        var secondValue = Next(IterationHint.None);
        Reset(oldPosition);
        return secondValue.IsDone;
    }

    public AbstractValue? First()
    {
        if (_cachedValues.Length != 0)
        {
            return _cachedValues[0];
        }

        var firstValue = Next(IterationHint.None);

        Reset();

        return firstValue.IsDone ? null : firstValue.Value;
    }

    public AbstractValue[] GetAllValues()
    {
        if (_currentPosition > _cachedValues.Length && _length != _cachedValues.Length)
        {
            throw new XPathException("Implementation error: Sequence Iterator has progressed.");
        }

        _cacheAllValues = true;

        var val = Next(IterationHint.None);
        while (!val.IsDone)
        {
            val = Next(IterationHint.None);
        }

        return _cachedValues;
    }

    public int GetLength()
    {
        return GetLength(false);
    }
    
    public int GetLength(bool onlyIfCheap)
    {
        if (_length != null)
        {
            // TODO: fix this, why does `_length!` not work? 
            return (int) _length;
        }

        if (onlyIfCheap)
        {
            return -1;
        }
        
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