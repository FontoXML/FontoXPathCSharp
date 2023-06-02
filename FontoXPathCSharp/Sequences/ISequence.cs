using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public interface ISequence
{
    delegate ISequence CallbackType(IReadOnlyList<AbstractValue?> values);

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
        return callback(firstValues);
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

    public ISequence Every(Func<AbstractValue?, ISequence> typeTest)
    {
        var iterator = GetValue();
        ISequence? typeTestResultIterator = null;
        var done = false;
        return SequenceFactory.CreateFromIterator(
            _ =>
            {
                while (!done)
                {
                    if (typeTestResultIterator == null)
                    {
                        var value = iterator(IterationHint.None);
                        if (value.IsDone)
                        {
                            done = true;
                            return IteratorResult<AbstractValue>.Ready(AtomicValue.TrueBoolean);
                        }

                        typeTestResultIterator = typeTest(value.Value);
                    }

                    var ebv = typeTestResultIterator.GetEffectiveBooleanValue();
                    typeTestResultIterator = null;
                    if (!ebv)
                    {
                        done = true;
                        return IteratorResult<AbstractValue>.Ready(AtomicValue.FalseBoolean);
                    }
                }

                return IteratorResult<AbstractValue>.Done();
            }
        );
    }

    public static Func<ISequence> CreateDoublyIterableSequence(ISequence sequence)
    {
        var savedValues = new List<IteratorResult<AbstractValue>>();
        var backingIterator = sequence.GetValue();
        return () =>
        {
            var i = 0;
            return SequenceFactory.CreateFromIterator(
                _ =>
                {
                    if (i < savedValues.Count) return savedValues[i++];
                    var val = backingIterator(IterationHint.None);
                    if (val.IsDone) return val;

                    if (i < savedValues.Count)
                    {
                        savedValues[i++] = val;
                    }
                    else
                    {
                        savedValues.Add(val);
                        i++;
                    }

                    return val;
                }
            );
        };
    }
}