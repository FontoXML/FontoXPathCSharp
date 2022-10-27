namespace FontoXPathCSharp;

public class IteratorResult<T>
{
    public readonly bool IsDone;
    public readonly T? Value;

    private IteratorResult(bool isDone, T? value)
    {
        IsDone = isDone;
        Value = value;
    }

    public static IteratorResult<T> Done()
    {
        return new IteratorResult<T>(true, default);
    }

    public static IteratorResult<T> Ready(T value)
    {
        return new IteratorResult<T>(false, value);
    }
}

[Flags]
public enum IterationHint
{
    None = 0,
    SkipDescendants = 1 << 0,
}

public delegate IteratorResult<T> Iterator<T>(IterationHint hint);

public static class IteratorUtils
{
    public static Iterator<T> SingleValueIterator<T>(T value)
    {
        var hasPassed = false;
        return _ =>
        {
            if (hasPassed) return IteratorResult<T>.Done();
            hasPassed = true;
            return IteratorResult<T>.Ready(value);
        };
    }

    public static Iterator<T> EmptyIterator<T>()
    {
        return _ => IteratorResult<T>.Done();
    }

    public static Iterator<T> ArrayIterator<T>(T[] values)
    {
        var i = 0;
        return _ => i >= values.Length
            ? IteratorResult<T>.Done()
            : IteratorResult<T>.Ready(values[i++]);
    }
}