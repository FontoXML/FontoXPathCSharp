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
}

public enum IterationHint
{
    None = 0,
    SkipDescendants = 1 << 0
}

public interface IIterator<T> where T: class
{
    public IteratorResult<T> Next(IterationHint hint);
}