namespace prscsharp;

#nullable enable

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

internal static class PrscSharp
{
    public delegate TR ParseFunc<out TR>(string input, int offset);

    public static ParseFunc<ParseResult<string>> Token(string token)
    {
        return (input, offset) =>
        {
            var endOffset = offset + token.Length;
            if (endOffset > input.Length)
                return new Err<string>(offset, new[] {token});
            if (input.Substring(offset, token.Length) == token)
                return new Ok<string>(endOffset, token);
            return new Err<string>(offset, new[] {token});
        };
    }

    public static ParseFunc<ParseResult<TR>> Map<T, TR>(ParseFunc<ParseResult<T>> parser, Func<T, TR> func)
    {
        return (input, offset) =>
        {
            var result = parser(input, offset);
            return result.Match<ParseResult<TR>>(
                value => new Ok<TR>(result.Offset, func(value)),
                (expected, fatal) => new Err<TR>(result.Offset, expected, fatal)
            );
        };
    }

    public static ParseFunc<ParseResult<T?>> Optional<T>(ParseFunc<ParseResult<T>> parser)
    {
        return (input, offset) =>
        {
            var result = parser(input, offset);
            return result.Match(
                value => new Ok<T?>(result.Offset, value),
                // NOTE: when not able to parse, we use the default value of T
                (_, _) => new Ok<T?>(result.Offset, default)
            );
        };
    }

    public static ParseFunc<ParseResult<T[]>> Star<T>(ParseFunc<ParseResult<T>> parser)
    {
        return (input, offset) =>
        {
            var results = new List<T>();
            var nextOffset = offset;

            while (true)
            {
                var result = parser(input, nextOffset);

                if (result.IsErr())
                {
                    var errorResult = (Err<T>) result;
                    if (errorResult.Fatal)
                        return new Err<T[]>(result.Offset, errorResult.Expected, errorResult.Fatal);
                    break;
                }

                results.Add(result.Unwrap());
                nextOffset = result.Offset;
            }

            return new Ok<T[]>(nextOffset, results.ToArray());
        };
    }

    public static ParseFunc<ParseResult<T>> Or<T>(ParseFunc<ParseResult<T>>[] parsers)
    {
        return (input, offset) =>
        {
            Err<T>? lastError = null;
            foreach (var parser in parsers)
            {
                var res = parser(input, offset);
                if (res.IsOk())
                    return res;

                var resError = (Err<T>) res;

                if (lastError == null || res.Offset > lastError.Offset)
                    lastError = resError;
                else if (res.Offset == lastError.Offset)
                    lastError.Expected = lastError.Expected.Concat(resError.Expected).ToArray();

                if (resError.Fatal)
                    break;
            }

            return lastError ?? new Err<T>(offset, Array.Empty<string>());
        };
    }

    public static ParseFunc<ParseResult<T>> Peek<T>(ParseFunc<ParseResult<T>> parser)
    {
        return (input, offset) =>
        {
            var result = parser(input, offset);
            return result.IsErr() ? result : new Ok<T>(offset, result.Unwrap());
        };
    }
}

internal static class Program
{
    public static void Main()
    {
        var parser = PrscSharp.Peek(PrscSharp.Token("A"));

        Console.WriteLine("Text: " + parser("AA", 0).Unwrap());
    }
}