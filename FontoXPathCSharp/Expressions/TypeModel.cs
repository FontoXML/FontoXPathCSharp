using FontoXPathCSharp.Expressions.DataTypes.Facets;
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
    private ExplicitTimezone? ExplicitTimezone { get; init; }

    private int? FractionDigits { get; init; }

    private string? MaxInclusive { get; init; }

    private string? MinInclusive { get; init; }

    private int? MinLength { get; init; }

    private WhitespaceHandling? Whitespace { get; init; }

    public TypeRestrictions(ExplicitTimezone? explicitTimezone = null, int? fractionDigits = null, string? maxInclusive = null, string? minInclusive = null, int? minLength = null, WhitespaceHandling? whitespace = null)
    {
        ExplicitTimezone = explicitTimezone;
        FractionDigits = fractionDigits;
        MaxInclusive  = maxInclusive;
        MinInclusive = minInclusive;
        MinLength = minLength;
        Whitespace = whitespace;
    }
}

public class TypeModel
{
    public TypeModel(Variety variety,
        ValueType type,
        TypeRestrictions? restrictionsByName,
        TypeModel? parentType,
        Func<string, bool>? validator,
        TypeFacetHandlers? typeFacetHandlers, TypeModel[] memberTypes)
    {
        MemberTypes = memberTypes;
        Parent = parentType;
        RestrictionsByName = restrictionsByName;
        Type = type;
        Validator = validator;
        Variety = variety;
        TypeFacetHandlers = typeFacetHandlers;
    }

    public TypeModel[] MemberTypes { get; }

    public TypeModel? Parent { get; }

    public TypeRestrictions? RestrictionsByName { get; }

    public TypeFacetHandlers? TypeFacetHandlers { get; }
    
    public ValueType Type { get; }

    public Func<string, bool>? Validator { get; }

    public Variety Variety { get; }
}