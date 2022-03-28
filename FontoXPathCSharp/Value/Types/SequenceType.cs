namespace FontoXPathCSharp.Value.Types;

public enum SequenceMultiplicity
{
    ZeroOrOne,
    OneOrMore,
    ZeroOrMore,
    ExactlyOne,
}

public class SequenceType
{
    public readonly ValueType ValueType;
    public readonly SequenceMultiplicity Multiplicity;

    public SequenceType(ValueType valueType, SequenceMultiplicity multiplicity)
    {
        ValueType = valueType;
        Multiplicity = multiplicity;
    }
}