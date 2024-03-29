namespace FontoXPathCSharp;

public static class CollectionExtensions
{
    public static TAccumulator Reduce<TSource, TAccumulator>(
        this IList<TSource> source,
        TAccumulator seed,
        Func<TAccumulator, TSource, int, IList<TSource>, TAccumulator> func)
    {
        return source.Select((v, i) => (v, i)).Aggregate(seed,
            (acc, valueAndIterator) => func(acc, valueAndIterator.v, valueAndIterator.i, source)
        );
    }

    public static TAccumulator Reduce<TSource, TAccumulator>(
        this IEnumerable<TSource> source,
        TAccumulator seed,
        Func<TAccumulator, TSource, int, TAccumulator> func)
    {
        return source.Select((v, i) => (v, i)).Aggregate(seed,
            (acc, valueAndIterator) => func(acc, valueAndIterator.v, valueAndIterator.i)
        );
    }


    // Replacements for JS' ReduceRight functions. 
    public static IList<TAccumulator> ReduceRight<TSource, TAccumulator>(
        this IList<TSource> source,
        IList<TAccumulator> seed,
        Func<IList<TAccumulator>, TSource, int, IList<TSource>, IList<TAccumulator>> func)
    {
        return source.Select((v, i) => (v, i)).Reverse().Aggregate(seed,
            (acc, valueAndIterator) => func(acc, valueAndIterator.v, valueAndIterator.i, source)
        );
    }

    public static TAccumulator ReduceRight<TSource, TAccumulator>(
        this IEnumerable<TSource> source,
        TAccumulator seed,
        Func<TAccumulator, TSource, int, TAccumulator> func)
    {
        var indexedAndReversed = source.Select((v, i) => (v, i)).Reverse();
        return indexedAndReversed.Aggregate(seed,
            (acc, valueAndIterator) => func(acc, valueAndIterator.v, valueAndIterator.i)
        );
    }

    public static IList<TAccumulator> ReduceRight<TSource, TAccumulator>(
        this IList<TSource> source,
        IList<TAccumulator> seed,
        Func<IList<TAccumulator>, TSource, IList<TAccumulator>> func)
    {
        return source.Reverse().Aggregate(seed, func);
    }
}