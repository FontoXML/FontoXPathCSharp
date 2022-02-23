namespace FontoXPathCSharp.Sequences;

public class SingletonSequence : ISequence
{
    private readonly Value _onlyValue;

    public SingletonSequence(Value onlyValue)
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

    public Value First()
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