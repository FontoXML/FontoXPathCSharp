namespace prscsharp;


public abstract class ParseResult<T>
{
    protected ParseResult(int offset)
    {
        Offset = offset;
    }

    public int Offset { get; }

    public abstract TR Match<TR>(Func<T, TR> ok, Func<string[], bool, TR> err);

    public void Match(Action<T> ok, Action<string[], bool> err)
    {
        Match(
            value =>
            {
                ok(value);
                return 0;
            },
            (expected, fatal) =>
            {
                err(expected, fatal);
                return 0;
            }
        );
    }

    public abstract bool IsOk();
    public abstract bool IsErr();

    public abstract T Unwrap();
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

    public override bool IsErr()
    {
        return false;
    }

    public override T Unwrap()
    {
        return Value;
    }
}

public class Err<T> : ParseResult<T>
{
    public string[] Expected;
    public readonly bool Fatal;

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

    public override bool IsErr()
    {
        return true;
    }

    public override T Unwrap()
    {
        throw new InvalidCastException("Called unwrap on Err result");
    }
}
