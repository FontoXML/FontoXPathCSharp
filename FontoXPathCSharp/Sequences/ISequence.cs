using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public interface ISequence : IEnumerable<AbstractValue>
{
    delegate ISequence CallbackType(IReadOnlyList<AbstractValue> values);

    bool IsEmpty();
    bool IsSingleton();
    AbstractValue? First();
    AbstractValue[] GetAllValues();
    int GetLength();
    bool GetEffectiveBooleanValue();

    Iterator<AbstractValue> GetValue();

    ISequence Filter(Func<AbstractValue, int, ISequence, bool> callback);
    ISequence Map(Func<AbstractValue, int, ISequence, AbstractValue> callback);
    ISequence MapAll(Func<AbstractValue[], ISequence> allvalues, IterationHint hint = IterationHint.None);

    public static ISequence ZipSingleton(IEnumerable<ISequence> sequences, CallbackType callback)
    {
        var firstValues = sequences.Select(x => x.First()).ToList();
        return callback(firstValues!);
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