namespace FontoXPathCSharp.Value.Types;

public enum SequenceMultiplicity
{
    ZeroOrOne,
    OneOrMore,
    ZeroOrMore,
    ExactlyOne
}


public class SequenceType
{
    public readonly SequenceMultiplicity Multiplicity;
    public readonly ValueType ValueType;

    public SequenceType(ValueType valueType, SequenceMultiplicity multiplicity)
    {
        ValueType = valueType;
        Multiplicity = multiplicity;
    }
}
