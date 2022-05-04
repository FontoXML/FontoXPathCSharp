using PrscSharp;
using static PrscSharp.PrscSharp;

namespace FontoXPathCSharp.Parsing;

public static class ParsingFunctions
{
    public static ParseFunc<ParseResult<T>> PrecededMultiple<TBefore, T>(
        IEnumerable<ParseFunc<ParseResult<TBefore>>> before,
        ParseFunc<ParseResult<T>> parser)
    {
        return before.Aggregate(parser, (current, b) => Preceded(b, current));
    }


    public static ParseFunc<ParseResult<TR>> Then3<T1, T2, T3, TR>(ParseFunc<ParseResult<T1>> parser1,
        ParseFunc<ParseResult<T2>> parser2, ParseFunc<ParseResult<T3>> parser3,
        Func<T1, T2, T3, TR> join)
    {
        return Then(parser1, Then(parser2, parser3, (b, c) => (b, c)), (a, bc) => join(a, bc.b, bc.c));
    }

    public static ParseFunc<ParseResult<T>> Alias<T>(T aliasedValue, params string[] tokenNames)
    {
        return Map(Or(tokenNames.Select(Token).ToArray()), _ => aliasedValue);
    }
}