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

    public Value? First()
    {
        return null;
    }

    public int GetLength()
    {
        return 0;
    }
}