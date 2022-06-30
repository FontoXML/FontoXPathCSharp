namespace PrscSharp;

public abstract class ParseResult<T>
{
    protected ParseResult(int offset)
    {
        Offset = offset;
    }

    public int Offset { get; }

    public abstract TR Match<TR>(Func<T, TR> ok, Func<string[], bool, TR> err);

    public abstract bool IsOk();

    public bool IsErr()
    {
        return !IsOk();
    }

    public T UnwrapOr(Func<string[], bool, T> callback)
    {
        return Match(value => value, callback);
    }

    public abstract T Unwrap();
    public abstract Err<T> UnwrapError();
}

public class Ok<T> : ParseResult<T>
{
    public Ok(int offset, T value) : base(offset)
    {
        Value = value;
    }

    private T Value { get; }

    public override TR Match<TR>(Func<T, TR> ok, Func<string[], bool, TR> err)
    {
        return ok(Value);
    }

    public override bool IsOk()
    {
        return true;
    }

    public override T Unwrap()
    {
        return Value;
    }

    public override Err<T> UnwrapError()
    {
        throw new InvalidCastException("Called UnwrapError on Ok result");
    }
}

public class Err<T> : ParseResult<T>
{
    public readonly bool Fatal;
    public string[] Expected;

    public Err(int offset, string[] expected, bool fatal = false) : base(offset)
    {
        Expected = expected;
        Fatal = fatal;
    }

    public override TR Match<TR>(Func<T, TR> ok, Func<string[], bool, TR> err)
    {
        return err(Expected, Fatal);
    }

    public override bool IsOk()
    {
        return false;
    }

    public override T Unwrap()
    {
        throw new InvalidCastException("Called unwrap on Err result");
    }

    public override Err<T> UnwrapError()
    {
        return this;
    }

    public override String ToString()
    {
        return "Error (" + Fatal + "), expected: " + string.Join(", ", Expected.Distinct());
    }
}
