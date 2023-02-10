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
    
    public static SequenceType StringToSequenceType(string input)
    {
        return input[^1] switch
        {
            '*' => new SequenceType(input[..^1].StringToValueType(), SequenceMultiplicity.ZeroOrMore),
            '?' => new SequenceType(input[..^1].StringToValueType(), SequenceMultiplicity.ZeroOrOne),
            '+' => new SequenceType(input[..^1].StringToValueType(), SequenceMultiplicity.OneOrMore),
            _ => new SequenceType(input.StringToValueType(), SequenceMultiplicity.ExactlyOne)
        };
    }

    public override string ToString()
    {
        return ValueType.Name() + Multiplicity switch
        {
            SequenceMultiplicity.ZeroOrMore => '*',
            SequenceMultiplicity.OneOrMore => '+',
            SequenceMultiplicity.ZeroOrOne => '?',
            _ => ""
        };
    }
}