namespace prscsharp;

#nullable enable

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