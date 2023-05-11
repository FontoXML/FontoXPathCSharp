namespace FontoXPathCSharp.Value.Types;

public class ParameterType : SequenceType
{
    public readonly bool IsEllipsis;

    public ParameterType(ValueType valueType, SequenceMultiplicity multiplicity) : base(valueType, multiplicity)
    {
        IsEllipsis = false;
    }
    
    public ParameterType(SequenceType sequenceType) : base(sequenceType.ValueType, sequenceType.Multiplicity)
    {
    }

    private ParameterType() : base(ValueType.None, SequenceMultiplicity.ZeroOrMore)
    {
        IsEllipsis = true;
    }

    public static ParameterType Ellipsis => new();
}