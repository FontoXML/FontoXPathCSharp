using System.Collections;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

internal class EmptySequence : ISequence
{
    private Iterator<AbstractValue> _value = hint => IteratorResult<AbstractValue>.Done();

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
        throw new NotImplementedException();
    }

    public ISequence Map(Func<AbstractValue, int, ISequence, AbstractValue> callback)
    {
        throw new NotImplementedException();
    }

    public ISequence MapAll(Func<AbstractValue[], ISequence> allvalues)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<AbstractValue> GetEnumerator()
    {
        throw new NotImplementedException();
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