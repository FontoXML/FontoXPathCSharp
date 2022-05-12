using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public static class SequenceFactory
{
    public static ISequence CreateFromIterator(Iterator<AbstractValue> iterator, int predictedLength)
    {
        return new IteratorBackedSequence(iterator, predictedLength);
    }

    public static ISequence CreateFromValue(AbstractValue? value)
    {
        if (value == null)
        {
            return new EmptySequence();
        }

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