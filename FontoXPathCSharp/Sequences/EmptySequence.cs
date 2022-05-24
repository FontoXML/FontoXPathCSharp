using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public class EmptySequence : ISequence
{
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

    public int GetLength()
    {
        return 0;
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