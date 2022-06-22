using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class BuiltInModelTypeDeclaration
{
    public BuiltInModelTypeDeclaration(
        Variety variety,
        ValueType name,
        ValueType? baseType = null,
        ValueType? type = null,
        ValueType[]? memberTypes = null,
        ValueType? parentType = null,
        TypeRestrictions? restrictions = null)
    {
        Variety = variety;
        Name = name;
        BaseType = baseType;
        Type = type;
        MemberTypes = memberTypes;
        ParentType = parentType;
        Restrictions = restrictions;
    }

    public Variety Variety { get; init; }
    public ValueType Name { get; init; }
    public ValueType? BaseType { get; init; }
    public ValueType? Type { get; init; }
    public ValueType[]? MemberTypes { get; init; }
    public ValueType? ParentType { get; init; }
    public TypeRestrictions? Restrictions { get; init; }
}