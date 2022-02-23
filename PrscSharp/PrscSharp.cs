using System.Text.RegularExpressions;

namespace prscsharp;

#nullable enable

public delegate TR ParseFunc<out TR>(string input, int offset);

public static class PrscSharp
{
    public static ParseFunc<ParseResult<string>> Token(string token)
    {
        return (input, offset) =>
        {
            var endOffset = offset + token.Length;
            if (endOffset > input.Length)
                return new Err<string>(offset, new[] { token });
            if (input.Substring(offset, token.Length) == token)
                return new Ok<string>(endOffset, token);
            return new Err<string>(offset, new[] { token });
        };
    }

    public static ParseFunc<ParseResult<string>> Regex(string regex)
    {
        return (input, offset) =>
        {
            Regex rx = new Regex(regex);
            Match match = rx.Match(input[offset..]);

            if (match.Success && match.Index == 0)
                return new Ok<string>(offset + match.Length, match.Value);
            return new Err<string>(offset, new[] { regex });
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
                    var errorResult = result.UnwrapError();
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

                var resError = res.UnwrapError();

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

    public static ParseFunc<ParseResult<T>> Then<T1, T2, T>(
        ParseFunc<ParseResult<T1>> parser1,
        ParseFunc<ParseResult<T2>> parser2,
        Func<T1, T2, T> join)
    {
        return (input, offset) =>
        {
            var r1 = parser1(input, offset);
            if (!r1.IsOk())
            {
                var r1Err = (Err<T1>)r1;
                return new Err<T>(r1Err.Offset, r1Err.Expected);
            }

            var r2 = parser2(input, r1.Offset);
            if (r2.IsOk()) return new Ok<T>(r2.Offset, join(r1.Unwrap(), r2.Unwrap()));

            var r2Err = (Err<T2>)r2;
            return new Err<T>(r2Err.Offset, r2Err.Expected);
        };
    }
}
