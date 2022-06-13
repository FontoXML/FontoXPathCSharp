namespace FontoXPathCSharp;

public class IteratorResult<T> where T : class
{
    public readonly bool IsDone;
    public readonly T? Value;

    public IteratorResult(bool isDone, T? value)
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

public enum IterationHint
{
    None = 0,
    SkipDescendants = 1 << 0
}

// public interface Iterator<T> where T : class
// {
//     public IteratorResult<T> Next(IterationHint hint);
// }

public delegate IteratorResult<T> Iterator<T>(IterationHint hint) where T : class;