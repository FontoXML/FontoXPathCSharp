using System.Collections;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

internal class EmptySequence : ISequence
{
    private readonly Iterator<AbstractValue> _value = hint => IteratorResult<AbstractValue>.Done();

    public bool IsEmpty()
    {
        return true;
    }

    public bool IsSingleton()
    {
        return false;
    }

    public AbstractValue? First()
    {
        return null;
    }

    public AbstractValue[] GetAllValues()
    {
        return Array.Empty<AbstractValue>();
    }

    public int GetLength()
    {
        return 0;
    }

    public Iterator<AbstractValue> GetValue()
    {
        return _value;
    }

    public ISequence Filter(Func<AbstractValue, int, ISequence, bool> callback)
    {
        return this;
    }

    public ISequence Map(Func<AbstractValue, int, ISequence, AbstractValue> callback)
    {
        return this;
    }

    public ISequence MapAll(Func<AbstractValue[], ISequence> allvalues, IterationHint hint)
    {
        return allvalues(Array.Empty<AbstractValue>());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<AbstractValue> GetEnumerator()
    {
        // TODO: I think this is correct, but not entirely sure?
        yield break;
    }

    public bool GetEffectiveBooleanValue()
    {
        return false;
    }

    public override string ToString()
    {
        return "<EmptySequence>[]";
    }
}