namespace FontoXPathCSharp.Value.Types;

public enum ValueType
{
    XsBoolean,
    XsString,
    XsNumeric,
    XsQName,
    Node,
    Function,
    None,
    XsInteger,
    Item,
    XsAnyAtomicType,
    XsUntypedAtomic,
    XsDecimal,
    XsDouble,
    XsFloat,
    Array,
    XsUnsignedShort,
    XsUnsignedByte,
    XsDuration,
    XsDateTime,
    XsTime,
    XsDate,
    XsGYearMonth,
    DocumentNode,
    ProcessingInstruction,
    Text,
    Attribute,
    Comment,
    Element,
    Map,
    XsError,
    XsDayTimeDuration,
    XsYearMonthDuration,
    XsNonNegativeInteger,
    XsPositiveInteger,
    XsUnsignedInt,
    XsUnsignedLong,
    XsShort,
    XsByte,
    XsInt,
    XsLong,
    XsNonPositiveInteger,
    XsNegativeInteger,
    XsEntity,
    XsEntities,
    XsNcName,
    XsIdRef,
    XsIdRefs,
    XsId,
    XsName,
    XsToken,
    XsNmToken,
    XsNmTokens,
    XsLanguage,
    XsNormalizedString,
    XsDateTimeStamp,
    XsNotation,
    XsAnyUri,
    XsBase64Binary,
    XsHexBinary,
    XsGMonth,
    XsGDay,
    XsGMonthDay,
    XsGYear,
    XsAnySimpleType
}

static class ValueTypeUtils
{
    public static ValueType StringToValueType(this string x)
    {
        return x switch
        {
            "array" => ValueType.Array,
            _ => throw new NotImplementedException("StringToValueType for '" + x + "'")
        };
    }
}