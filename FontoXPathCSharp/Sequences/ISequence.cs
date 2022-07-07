using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public interface ISequence : IEnumerable<AbstractValue>
{
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

    delegate ISequence CallbackType(IEnumerable<AbstractValue> values);

    public static ISequence ZipSingleton(IEnumerable<ISequence> sequences, CallbackType callback)
    {
        var firstValues = sequences.Select(x => x.First());
        return callback(firstValues!);
    }
}