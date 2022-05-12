using System.Collections;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public class SingletonSequence : ISequence
{
    private readonly AbstractValue _onlyValue;

    public SingletonSequence(AbstractValue onlyValue)
    {
        _onlyValue = onlyValue;
    }

    public IEnumerator<AbstractValue> GetEnumerator()
    {
        return new[] {_onlyValue}.ToList().GetEnumerator();
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
        return new[] {_onlyValue};
    }

    public int GetLength()
    {
        return 1;
    }

    public override string ToString()
    {
        return "<SingletonSequence>[" + _onlyValue + "]";
    }
}