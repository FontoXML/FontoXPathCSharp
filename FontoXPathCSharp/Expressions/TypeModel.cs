using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public enum WhitespaceHandling
{
    Replace,
    Collapse,
    Preserve
}

public enum ExplicitTimezone
{
    Optional,
    Required
}

public class TypeRestrictions
{
    public ExplicitTimezone? ExplicitTimezone = null;
    public int? FractionDigits = null;
    public string? MaxInclusive = null;
    public string? MinInclusive = null;
    public int? MinLength = null;
    public WhitespaceHandling? Whitespace = null;
}

public class TypeModel
{
    public TypeModel(TypeModel[] memberTypes,
        TypeModel? parent,
        TypeRestrictions restrictionsByName,
        ValueType type,
        Func<string, bool> validator,
        Variety variety)
    {
        MemberTypes = memberTypes;
        Parent = parent;
        RestrictionsByName = restrictionsByName;
        Type = type;
        Validator = validator;
        Variety = variety;
    }

    public TypeModel[] MemberTypes { get; }

    public TypeModel? Parent { get; }

    public TypeRestrictions? RestrictionsByName { get; }

    public ValueType Type { get; }

    public Func<string, bool> Validator { get; }

    public Variety Variety { get; }
}