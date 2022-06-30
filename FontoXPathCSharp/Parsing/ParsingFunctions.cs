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

    public static ParseFunc<TR> Then3<T1, T2, T3, TR>(ParseFunc<T1> parser1,
        ParseFunc<T2> parser2, ParseFunc<T3> parser3,
        Func<T1, T2, T3, TR> join)
    {
        return Then(parser1, Then(parser2, parser3, (b, c) => (b, c)), (a, bc) => join(a, bc.b, bc.c));
    }

    public static ParseFunc<T> Alias<T>(T aliasedValue, params string[] tokenNames)
    {
        return Map(Or(tokenNames.Select(Token).ToArray()), _ => aliasedValue);
    }
}