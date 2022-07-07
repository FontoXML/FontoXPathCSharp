namespace FontoXPathCSharp;

public class IteratorResult<T> where T : class
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
        return new IteratorResult<T>(true, null);
    }

    public static IteratorResult<T> Ready(T value)
    {
        return new IteratorResult<T>(false, value);
    }
}

[Flags]
public enum IterationHint
{
    None,
    SkipDescendants
}

public delegate IteratorResult<T> Iterator<T>(IterationHint hint) where T : class;

public static class IteratorUtils
{
    public static Iterator<T> SingleValueIterator<T>(T value) where T : class
    {
        var hasPassed = false;
        return _ =>
        {
            if (hasPassed) return IteratorResult<T>.Done();
            hasPassed = true;
            return IteratorResult<T>.Ready(value);
        };
    }

    public static Iterator<T> EmptyIterator<T>() where T : class
    {
        return _ => IteratorResult<T>.Done();
    }

    public static Iterator<T> ArrayIterator<T>(T[] values) where T : class
    {
        var i = 0;
        return _ => i >= values.Length
            ? IteratorResult<T>.Done()
            : IteratorResult<T>.Ready(values[i++]);
    }
}