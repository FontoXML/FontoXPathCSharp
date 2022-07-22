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

    public static ISequence ConcatSequences(ISequence[] sequences)
    {
        var i = 0;
        Iterator<AbstractValue>? iterator = null;
        var isFirst = true;
        return SequenceFactory.CreateFromIterator(
            hint =>
            {
                while (i < sequences.Length)
                {
                    if (iterator == null)
                    {
                        iterator = sequences[i].GetValue();
                        isFirst = true;
                    }

                    var value = iterator(isFirst ? IterationHint.None : hint);
                    isFirst = false;
                    if (value.IsDone)
                    {
                        i++;
                        iterator = null;
                        continue;
                    }

                    return value;
                }

                return IteratorResult<AbstractValue>.Done();
            }
        );
    }
}