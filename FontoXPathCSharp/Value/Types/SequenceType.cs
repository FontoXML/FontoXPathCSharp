namespace FontoXPathCSharp.Value.Types;

public enum SequenceMultiplicity
{
    ZeroOrOne,
    OneOrMore,
    ZeroOrMore,
    ExactlyOne
}

public static class SequenceMultiplicityStuff
{
    public static string Postfix(this SequenceMultiplicity mult)
    {
        return mult switch
        {
            SequenceMultiplicity.ZeroOrMore => "*",
            SequenceMultiplicity.OneOrMore => "+",
            SequenceMultiplicity.ZeroOrOne => "?",
            SequenceMultiplicity.ExactlyOne => "",
            _ => throw new ArgumentOutOfRangeException(nameof(mult), mult, null)
        };
    }
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

    public override string ToString()
    {
        return ValueType.Name() + Multiplicity.Postfix();
    }
}