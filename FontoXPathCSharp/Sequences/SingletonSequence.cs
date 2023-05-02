using System.Collections;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

internal class SingletonSequence : ISequence
{
    private readonly AbstractValue[] _onlyValue;
    private readonly Iterator<AbstractValue> _value;

    public SingletonSequence(AbstractValue onlyValue)
    {
        _onlyValue = new[] { onlyValue };
        _value = IteratorUtils.SingleValueIterator(onlyValue);
    }

    public IEnumerator<AbstractValue> GetEnumerator()
    {
        return _onlyValue.ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool IsEmpty()
    {
        return false;
    }

    public bool IsSingleton()
    {
        return true;
    }

    public AbstractValue First()
    {
        return _onlyValue[0];
    }

    public AbstractValue[] GetAllValues()
    {
        return _onlyValue;
    }

    public int GetLength()
    {
        return 1;
    }

    public Iterator<AbstractValue> GetValue()
    {
        return _value;
    }

    public ISequence Filter(Func<AbstractValue, int, ISequence, bool> callback)
    {
        return callback(_onlyValue[0], 0, this) ? this : SequenceFactory.CreateEmpty();
    }

    public ISequence Map(Func<AbstractValue, int, ISequence, AbstractValue> callback)
    {
        return SequenceFactory.CreateFromValue(callback(_onlyValue[0], 0, this));
    }

    public ISequence MapAll(Func<AbstractValue[], ISequence> callback, IterationHint hint)
    {
        return callback(GetAllValues());
    }

    public bool GetEffectiveBooleanValue()
    {
        return _onlyValue[0].GetEffectiveBooleanValue();
    }
}