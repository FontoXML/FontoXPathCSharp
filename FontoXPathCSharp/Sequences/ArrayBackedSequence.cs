namespace FontoXPathCSharp.Sequences;

public class ArrayBackedSequence : ISequence
{
    private readonly Value[] _values;

    public ArrayBackedSequence(Value[] values)
    {
        _values = values;
    }

    public bool IsEmpty()
    {
        return false;
    }

    public bool IsSingleton()
    {
        return false;
    }

    public Value? First()
    {
        return _values.Length == 0 ? null : _values[0];
    }

    public int GetLength()
    {
        return _values.Length;
    }
}