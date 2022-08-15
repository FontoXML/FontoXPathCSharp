using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public static class ParsingFunctions
{
    public static ParseFunc<T> PrecededMultiple<TBefore, T>(
        IEnumerable<ParseFunc<TBefore>> before,
        ParseFunc<T> parser)
    {
        return before.Reverse().Aggregate(parser, (current, b) => Preceded(b, current));
    }

    public static ParseFunc<T> FollowedMultiple<T, TAfter>(
        ParseFunc<T> parser,
        IEnumerable<ParseFunc<TAfter>> after)
    {
        return after.Aggregate(parser, Followed);
    }

    public static ParseFunc<TR> Then3<T1, T2, T3, TR>(
        ParseFunc<T1> parser1,
        ParseFunc<T2> parser2,
        ParseFunc<T3> parser3,
        Func<T1, T2, T3, TR> join)
    {
        return Then(parser1, Then(parser2, parser3, (b, c) => (b, c)), (a, bc) => join(a, bc.b, bc.c));
    }

    public static ParseFunc<TR> Then4<T1, T2, T3, T4, TR>(
        ParseFunc<T1> parser1,
        ParseFunc<T2> parser2,
        ParseFunc<T3> parser3,
        ParseFunc<T4> parser4,
        Func<T1, T2, T3, T4, TR> join)
    {
        return Then(
            Then(
                Then(parser1, parser2,
                    (a, b) => (a, b)
                ),
                parser3,
                (ab, c) => (ab.a, ab.b, c)
            ),
            parser4,
            (abc, d) => join(abc.a, abc.b, abc.c, d)
        );
    }

    public static ParseFunc<TR> Then5<T1, T2, T3, T4, T5, TR>(
        ParseFunc<T1> parser1,
        ParseFunc<T2> parser2,
        ParseFunc<T3> parser3,
        ParseFunc<T4> parser4,
        ParseFunc<T5> parser5,
        Func<T1, T2, T3, T4, T5, TR> join)
    {
        return Then(
            Then(
                Then(
                    Then(parser1, parser2,
                        (a, b) => (a, b)
                    ),
                    parser3,
                    (ab, c) => (ab.a, ab.b, c)
                ),
                parser4,
                (abc, d) => (abc.a, abc.b, abc.c, d)
            ),
            parser5,
            (abcd, e) => join(abcd.a, abcd.b, abcd.c, abcd.d, e)
        );
    }

    public static ParseFunc<T> Alias<T>(T aliasedValue, params string[] tokenNames)
    {
        return Map(Or(tokenNames.Select(Token).ToArray()), _ => aliasedValue);
    }
}