#nullable enable

public abstract class ParseResult<O>
{
    public int Offset { get; private set; }

    public ParseResult(int offset)
    {
        this.Offset = offset;
    }

    public abstract T Match<T>(Func<O, T> Ok, Func<string[], bool, T> Err);

    public void Match(Action<O> Ok, Action<string[], bool> Err)
    {
        this.Match<int>(
            Ok: (value) => { Ok(value); return 0; },
            Err: (expected, fatal) => { Err(expected, fatal); return 0; }
        );
    }

    public abstract bool IsOk();
    public abstract bool IsErr();

    public abstract O Unwrap();
}

public class Ok<O> : ParseResult<O>
{
    public O Value { get; private set; }

    public Ok(int offset, O value) : base(offset)
    {
        this.Value = value;
    }

    public override T Match<T>(Func<O, T> Ok, Func<string[], bool, T> _Err)
    {
        return Ok(this.Value);
    }

    public override bool IsOk()
    {
        return true;
    }

    public override bool IsErr()
    {
        return false;
    }

    public override O Unwrap()
    {
        return this.Value;
    }
}

public class Err<O> : ParseResult<O>
{
    public string[] Expected;
    public bool Fatal;


    public Err(int offset, string[] expected, bool fatal = false) : base(offset)
    {
        this.Expected = expected;
        this.Fatal = fatal;
    }

    public override T Match<T>(Func<O, T> _Ok, Func<string[], bool, T> Err)
    {
        return Err(this.Expected, this.Fatal);
    }

    public override bool IsOk()
    {
        return false;
    }

    public override bool IsErr()
    {
        return true;
    }

    public override O Unwrap()
    {
        throw new InvalidCastException("Called unwrap on Err result");
    }
}


class Prsc
{
    public delegate K ParseFunc<out K>(string input, int offset);

    public static ParseFunc<ParseResult<string>> token(string token)
    {
        return (string input, int offset) =>
        {
            int endOffset = offset + token.Length;
            if (endOffset >= input.Length)
                return new Err<string>(offset, new string[] { token });
            if (input.Substring(offset, token.Length) == token)
                return new Ok<string>(endOffset, token);
            return new Err<string>(offset, new string[] { token });
        };
    }

    public static ParseFunc<ParseResult<U>> map<T, U>(ParseFunc<ParseResult<T>> parser, Func<T, U> func)
    {
        return (string input, int offset) =>
        {
            ParseResult<T> result = parser(input, offset);
            return result.Match<ParseResult<U>>(
              Ok: (value) => new Ok<U>(result.Offset, func(value)),
              Err: (expected, fatal) => new Err<U>(result.Offset, expected, fatal)
            );
        };
    }

    public static ParseFunc<ParseResult<T?>> optional<T>(ParseFunc<ParseResult<T>> parser)
    {
        return (string input, int offset) =>
        {
            ParseResult<T> result = parser(input, offset);
            return result.Match(
                Ok: (value) => new Ok<T?>(result.Offset, value),
                // NOTE: when not able to parse, we use the default value of T
                Err: (_expected, _fatal) => new Ok<T?>(result.Offset, default(T))
            );
        };
    }

    public static ParseFunc<ParseResult<T[]>> star<T>(ParseFunc<ParseResult<T>> parser)
    {
        return (string input, int offset) =>
        {
            List<T> results = new List<T>();
            int nextOffset = offset;

            while (true)
            {
                ParseResult<T> result = parser(input, nextOffset);

                if (result.IsErr())
                {
                    Err<T> errorResult = (Err<T>)result;
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

    public static ParseFunc<ParseResult<T>> or<T>(ParseFunc<ParseResult<T>>[] parsers)
    {
        return (input, offset) =>
        {
            Err<T>? lastError = null;
            foreach (var parser in parsers)
            {
                var res = parser(input, offset);
                if (res.IsOk())
                {
                    return res;
                }

                var resError = (Err<T>)res;

                if (lastError == null || res.Offset > lastError.Offset)
                {
                    lastError = resError;
                }
                else if (res.Offset == lastError.Offset)
                {
                    lastError.Expected = lastError.Expected.Concat(resError.Expected).ToArray();
                }
                if (resError.Fatal)
                {
                    break;
                }
            }
            return lastError != null ? lastError : new Err<T>(offset, Array.Empty<string>(), false);
        };
    }
}



class Program
{
    public static void Main(string[] args)
    {
        var parser = Prsc.star(Prsc.or(new[]
        {
            Prsc.token("A"), Prsc.token("B")
        }));
        
        Console.WriteLine("Text: " + string.Join(", ", parser("AA", 0).Unwrap()));
        Console.WriteLine("Text: " + string.Join(", ", parser("BA", 0).Unwrap()));
    }
}
