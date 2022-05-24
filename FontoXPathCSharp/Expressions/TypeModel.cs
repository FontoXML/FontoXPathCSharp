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
    public WhitespaceHandling? Whitespace = null;
    public ExplicitTimezone? ExplicitTimezone = null;
    public int? FractionDigits = null;
    public int? MinLength = null;
    public string? MinInclusive = null;
    public string? MaxInclusive = null;
}

public class TypeModel
{
    private readonly TypeModel[] _memberTypes;
    private readonly TypeModel? _parent;
    private readonly TypeRestrictions? _restrictionsByName;
    private readonly ValueType _type;
    private readonly Func<string, bool> _validator;
    private readonly Variety _variety;
    
    public TypeModel(TypeModel[] memberTypes,
        TypeModel? parent,
        TypeRestrictions restrictionsByName,
        ValueType type,
        Func<string, bool> validator,
        Variety variety)
    {
        _memberTypes = memberTypes;
        _parent = parent;
        _restrictionsByName = restrictionsByName;
        _type = type;
        _validator = validator;
        _variety = variety;
    }

    public TypeModel[] MemberTypes => _memberTypes;

    public TypeModel? Parent => _parent;

    public TypeRestrictions? RestrictionsByName => _restrictionsByName;

    public ValueType Type => _type;

    public Func<string, bool> Validator => _validator;

    public Variety Variety => _variety;
}