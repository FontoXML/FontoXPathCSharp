using System.Collections;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

internal class SingletonSequence : ISequence
{
    private readonly AbstractValue _onlyValue;

    public SingletonSequence(AbstractValue onlyValue)
    {
        _onlyValue = onlyValue;
    }

    public IEnumerator<AbstractValue> GetEnumerator()
    {
        return new[] { _onlyValue }.ToList().GetEnumerator();
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
        return _onlyValue;
    }

    public AbstractValue[] GetAllValues()
    {
        return new[] { _onlyValue };
    }

    public int GetLength()
    {
        return 1;
    }

    public Iterator<AbstractValue> GetValue()
    {
        return IteratorUtils.SingleValueIterator(_onlyValue);
    }

    public ISequence Filter(Func<AbstractValue, int, ISequence, bool> callback)
    {
        return callback(_onlyValue, 0, this) ? this : SequenceFactory.CreateEmpty();
    }

    public ISequence Map(Func<AbstractValue, int, ISequence, AbstractValue> callback)
    {
        return SequenceFactory.CreateFromValue(callback(_onlyValue, 0, this));
    }

    public ISequence MapAll(Func<AbstractValue[], ISequence> callback, IterationHint hint)
    {
        return callback(GetAllValues());
    }

    public bool GetEffectiveBooleanValue()
    {
        return _onlyValue.GetEffectiveBooleanValue();
    }
}