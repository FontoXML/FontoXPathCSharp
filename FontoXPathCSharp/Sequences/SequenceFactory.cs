using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public static class SequenceFactory
{
    public static ISequence CreateFromIterator<T>(Iterator<T> iterator, int? predictedLength = null) where T: AbstractValue
    {
        Iterator<AbstractValue> abstractValueIterator = hint =>
        {
            var result = iterator(hint);
            if (result.IsDone) return IteratorResult<AbstractValue>.Done();
            return IteratorResult<AbstractValue>.Ready(result.Value!);
        };
        return CreateFromIterator(abstractValueIterator, predictedLength);
    }
    
    public static ISequence CreateFromIterator(Iterator<AbstractValue> iterator, int? predictedLength = null)
    {
        return new IteratorBackedSequence(iterator, predictedLength);
    }

    public static ISequence CreateFromValue(AbstractValue? value)
    {
        if (value == null) return new EmptySequence();

        return new SingletonSequence(value);
    }

    public static ISequence CreateFromArray(AbstractValue[] values)
    {
        return values.Length switch
        {
            0 => new EmptySequence(),
            1 => new SingletonSequence(values[0]),
            _ => new ArrayBackedSequence(values)
        };
    }

    public static ISequence CreateEmpty()
    {
        return new EmptySequence();
    }
}