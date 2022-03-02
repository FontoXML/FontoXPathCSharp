namespace FontoXPathCSharp.Sequences;

using Value;

public class SingletonSequence : ISequence
{
    private readonly AbstractValue _onlyValue;

    public SingletonSequence(AbstractValue onlyValue)
    {
        _onlyValue = onlyValue;
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

    public int GetLength()
    {
        return 1;
    }

    public override string ToString()
    {
        return "<SingletonSequence>[" + _onlyValue + "]";
    }
}