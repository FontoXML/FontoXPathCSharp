namespace FontoXPathCSharp.Expressions.DataTypes.Facets;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

public class TypeFacetHandlers
{
}

public class TypeFacets
{
    public static TypeFacetHandlers? GetFacetsByDataType(ValueType valueType)
    {
        return new TypeFacetHandlers();
        // TODO: Implement facet handlers,
        // Similar to validators, these are too little of the spec to matter for now.
        
        // switch (valueType)
        // {
        //     case ValueType.XsString:
        //     case ValueType.XsBoolean:
        //     case ValueType.XsFloat:
        //     case ValueType.XsDouble:
        //     case ValueType.XsHexBinary:
        //     case ValueType.XsBase64Binary:
        //     case ValueType.XsAnyUri:
        //     case ValueType.XsQName:
        //     case ValueType.XsNotation:
        //     case ValueType.XsDuration:
        //         return new TypeFacetHandlers();
        //     case ValueType.XsDecimal:
        //         return new TypeFacetHandlers(
        //             fractionDigits: validateFractionDigits,
        //             maxInclusive: createMaxInclusiveFacet(decimalComparator),
        //             maxExclusive: createMaxExclusiveFacet(decimalComparator),
        //             minInclusive: createMinInclusiveFacet(decimalComparator),
        //             minExclusive: createMinExclusiveFacet(decimalComparator),
        //         );
        //     case ValueType.XsDateTime:
        //     case ValueType.XsTime:
        //     case ValueType.XsDate:
        //     case ValueType.XsGYearMonth:
        //     case ValueType.XsGYear:
        //     case ValueType.XsGMonthDay:
        //     case ValueType.XsGDay:
        //     case ValueType.XsGMonth:
        //         return new TypeFacetHandlers( explicitTimezone: validateExplicitTimeZone );
        //     default:
        //         return null;
        // }
    }
}