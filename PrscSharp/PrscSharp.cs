using System.Text.RegularExpressions;

namespace PrscSharp;

#nullable enable

public delegate ParseResult<TR> ParseFunc<TR>(string input, int offset);

public static class PrscSharp
{
    public static readonly ParseFunc<object> End = (input, offset) => input.Length == offset
        ? new Ok<object>(offset, 0) // Ignore value here, return type cannot be void.
        // : new Ok<object>(offset, 1); // TODO: Uncomment when the offsets are working correctly.
        : new Err<object>(offset, new[] { $"End of input. Offset: {offset}, input length: {input.Length}" });

    public static ParseFunc<string> Token(string token)
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

    public static ParseFunc<string> Regex(string regex)
    {
        return (input, offset) =>
        {
            var rx = new Regex(regex);
            var match = rx.Match(input[offset..]);

            if (match.Success && match.Index == 0)
                return new Ok<string>(offset + match.Length, match.Value);
            return new Err<string>(offset, new[] { regex });
        };
    }

    public static ParseFunc<TR> Map<T, TR>(ParseFunc<T> parser, Func<T, TR> func)
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

    public static ParseFunc<T?> Optional<T>(ParseFunc<T> parser) where T : class
    {
        return (input, offset) =>
        {
            var result = parser(input, offset);
            return result.Match(
                value => new Ok<T?>(result.Offset, value),
                (_, _) => new Ok<T?>(result.Offset, null)
            );
        };
    }

    public static ParseFunc<T?> OptionalDefaultValue<T>(ParseFunc<T> parser) where T : struct
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

    public static ParseFunc<T[]> Star<T>(ParseFunc<T> parser)
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

    public static ParseFunc<T> Or<T>(params ParseFunc<T>[] parsers)
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

    public static ParseFunc<T> Peek<T>(ParseFunc<T> parser)
    {
        return (input, offset) =>
        {
            var result = parser(input, offset);
            return result.IsErr() ? result : new Ok<T>(offset, result.Unwrap());
        };
    }

    public static ParseFunc<T?> Not<T>(ParseFunc<T> parser, string[] expected)
    {
        return (input, offset) =>
        {
            var result = parser(input, offset);
            if (result.IsErr()) return new Ok<T?>(offset, default);
            return new Err<T?>(offset, expected);
        };
    }

    public static ParseFunc<TR> Then<T1, T2, TR>(
        ParseFunc<T1> parser1,
        ParseFunc<T2> parser2,
        Func<T1, T2, TR> join)
    {
        return (input, offset) =>
        {
            var r1 = parser1(input, offset);
            if (!r1.IsOk())
            {
                var r1Err = (Err<T1>)r1;
                return new Err<TR>(r1Err.Offset, r1Err.Expected);
            }

            var r2 = parser2(input, r1.Offset);
            if (r2.IsOk()) return new Ok<TR>(r2.Offset, join(r1.Unwrap(), r2.Unwrap()));

            var r2Err = (Err<T2>)r2;
            return new Err<TR>(r2Err.Offset, r2Err.Expected);
        };
    }


    public static ParseFunc<T[]> Plus<T>(ParseFunc<T> parser)
    {
        return Then(parser, Star(parser), (x, xs) => xs.Prepend(x).ToArray());
    }

    public static ParseFunc<T> Preceded<TBefore, T>(
        ParseFunc<TBefore> before,
        ParseFunc<T> parser)
    {
        return Then(before, parser, (_, x) => x);
    }

    public static ParseFunc<T> Followed<T, TAfter>(
        ParseFunc<T> parser,
        ParseFunc<TAfter> after
    )
    {
        return Then(parser, after, (x, _) => x);
    }

    public static ParseFunc<T> Delimited<T, TBefore, TAfter>(
        ParseFunc<TBefore> before,
        ParseFunc<T> parser,
        ParseFunc<TAfter> after)
    {
        return Preceded(before, Followed(parser, after));
    }

    public static ParseFunc<T> Surrounded<T, TAround>(
        ParseFunc<T> parser,
        ParseFunc<TAround> around)
    {
        return Delimited(around, parser, around);
    }

    public static T1 First<T1, T2>(T1 x, T2 y)
    {
        return x;
    }

    public static ParseFunc<T> Complete<T>(ParseFunc<T> parser)
    {
        return Then(parser, End, First);
    }

    public static ParseResult<T> OkWithValue<T>(int offset, T value)
    {
        return new Ok<T>(offset, value);
    }
}