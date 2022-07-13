using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.EvaluationUtils;

public static class SequenceUtils
{
    public static ISequence ZipSingleton(ISequence[] sequences, Func<AbstractValue?[], ISequence> callback)
    {
        var firstValues = sequences.Select(seq => seq.First());
        return callback(firstValues.ToArray());
    }
}