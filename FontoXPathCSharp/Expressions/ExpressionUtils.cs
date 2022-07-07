using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public static class ExpressionUtils
{
    public static ISequence ConcatSequences(IEnumerable<ISequence> sequencesContainer)
    {
        var sequences = sequencesContainer.ToArray();
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