using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Builtins;

public class BuiltInTypeModels
{
    private readonly BuiltInModelTypeDeclaration[] _builtinModels;

    // Resharper disable all
    // The above is to make sure the parameter names are not removed on cleanup.
    private BuiltInTypeModels()
    {
        _builtinModels = new BuiltInModelTypeDeclaration[]
        {
            new(variety: Variety.Primitive, name: ValueType.Item),
            // anyAtomicType
            new(
                variety: Variety.Primitive,
                name: ValueType.XsAnyAtomicType,
                parentType: ValueType.Item,
                restrictions: new TypeRestrictions(whitespace: WhitespaceHandling.Preserve)
            ),
            // untypedAtomic
            new(variety: Variety.Primitive,
                name: ValueType.XsUntypedAtomic,
                parentType: ValueType.XsAnyAtomicType),

            // string
            new(
                variety: Variety.Primitive,
                name: ValueType.XsString,
                parentType: ValueType.XsAnyAtomicType
            ),

            // boolean
            new(
                variety: Variety.Primitive,
                name: ValueType.XsBoolean,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // decimal
            new(
                variety: Variety.Primitive,
                name: ValueType.XsDecimal,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // float
            new(
                variety: Variety.Primitive,
                name: ValueType.XsFloat,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // double
            new(
                variety: Variety.Primitive,
                name: ValueType.XsDouble,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // duration
            new(
                variety: Variety.Primitive,
                name: ValueType.XsDuration,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // dateTime
            new(
                variety: Variety.Primitive,
                name: ValueType.XsDateTime,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    explicitTimezone: ExplicitTimezone.Optional,
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // time
            new(
                variety: Variety.Primitive,
                name: ValueType.XsTime,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    explicitTimezone: ExplicitTimezone.Optional,
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // date
            new(
                variety: Variety.Primitive,
                name: ValueType.XsDate,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    explicitTimezone: ExplicitTimezone.Optional,
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // gYearMonth
            new(
                variety: Variety.Primitive,
                name: ValueType.XsGYearMonth,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    explicitTimezone: ExplicitTimezone.Optional,
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // gYear
            new(
                variety: Variety.Primitive,
                name: ValueType.XsGYear,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    explicitTimezone: ExplicitTimezone.Optional,
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // gMonthDay
            new(
                variety: Variety.Primitive,
                name: ValueType.XsGMonthDay,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    explicitTimezone: ExplicitTimezone.Optional,
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // gDay
            new(
                variety: Variety.Primitive,
                name: ValueType.XsGDay,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    explicitTimezone: ExplicitTimezone.Optional,
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // gMonth
            new(
                variety: Variety.Primitive,
                name: ValueType.XsGMonth,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    explicitTimezone: ExplicitTimezone.Optional,
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // hexBinary
            new(
                variety: Variety.Primitive,
                name: ValueType.XsHexBinary,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // base64Binary
            new(
                variety: Variety.Primitive,
                name: ValueType.XsBase64Binary,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // anyURI
            new(
                variety: Variety.Primitive,
                name: ValueType.XsAnyUri,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // QName
            new(
                variety: Variety.Primitive,
                name: ValueType.XsQName,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // NOTATION
            new(
                variety: Variety.Primitive,
                name: ValueType.XsNotation,
                parentType: ValueType.XsAnyAtomicType,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // dateTimeStamp
            new(
                variety: Variety.Derived,
                name: ValueType.XsDateTimeStamp,
                baseType: ValueType.XsDateTime,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse, // fixed
                    explicitTimezone: ExplicitTimezone.Required // fixed
                )
            ),

            // normalizedString
            new(
                variety: Variety.Derived,
                name: ValueType.XsNormalizedString,
                baseType: ValueType.XsString,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Replace
                )
            ),

            // token
            new(
                variety: Variety.Derived,
                name: ValueType.XsToken,
                baseType: ValueType.XsNormalizedString,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // language (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsLanguage,
                baseType: ValueType.XsToken,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // NMTOKEN (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsNmToken,
                baseType: ValueType.XsToken,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // NMTOKENS
            new(
                variety: Variety.List,
                name: ValueType.XsNmTokens,
                type: ValueType.XsNmToken,
                restrictions: new TypeRestrictions(
                    minLength: 1,
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // Name (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsName,
                baseType: ValueType.XsToken,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // NCName (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsNcName,
                baseType: ValueType.XsName,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // ID (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsId,
                baseType: ValueType.XsNcName,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // IDREF (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsIdRef,
                baseType: ValueType.XsNcName,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // IDREFS
            new(
                variety: Variety.List,
                name: ValueType.XsIdRefs,
                type: ValueType.XsIdRef,
                restrictions: new TypeRestrictions(
                    minLength: 1,
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // ENTITY (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsEntity,
                baseType: ValueType.XsNcName,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // ENTITIES
            new(
                variety: Variety.List,
                name: ValueType.XsEntities,
                type: ValueType.XsEntity,
                restrictions: new TypeRestrictions(
                    minLength: 1,
                    whitespace: WhitespaceHandling.Collapse
                )
            ),

            // integer (TODO: implement pattern)
            new(
                variety: Variety.Primitive,
                name: ValueType.XsInteger,
                parentType: ValueType.XsDecimal,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // nonPositiveInteger (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsNonPositiveInteger,
                baseType: ValueType.XsInteger,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    maxInclusive: "0",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // negativeInteger (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsNegativeInteger,
                baseType: ValueType.XsNonPositiveInteger,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    maxInclusive: "-1",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // long (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsLong,
                baseType: ValueType.XsInteger,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    maxInclusive: "9223372036854775807",
                    minInclusive: "-9223372036854775808",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // int (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsInt,
                baseType: ValueType.XsLong,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    maxInclusive: "2147483647",
                    minInclusive: "-2147483648",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // short (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsShort,
                baseType: ValueType.XsInt,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    maxInclusive: "32767",
                    minInclusive: "-32768",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // byte (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsByte,
                baseType: ValueType.XsShort,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    maxInclusive: "127",
                    minInclusive: "-128",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // nonNegativeInteger (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsNonNegativeInteger,
                baseType: ValueType.XsInteger,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    minInclusive: "0",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // unsignedLong (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsUnsignedLong,
                baseType: ValueType.XsNonNegativeInteger,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    maxInclusive: "18446744073709551615",
                    minInclusive: "0",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // unsignedInt (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsUnsignedInt,
                baseType: ValueType.XsUnsignedLong,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    maxInclusive: "4294967295",
                    minInclusive: "0",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // unsignedShort (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsUnsignedShort,
                baseType: ValueType.XsUnsignedInt,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    maxInclusive: "65535",
                    minInclusive: "0",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // unsignedByte (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsUnsignedByte,
                baseType: ValueType.XsUnsignedShort,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    maxInclusive: "255",
                    minInclusive: "0",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // positiveInteger (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsPositiveInteger,
                baseType: ValueType.XsNonNegativeInteger,
                restrictions: new TypeRestrictions(
                    fractionDigits: 0, // fixed
                    minInclusive: "1",
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // yearMonthDuration (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsYearMonthDuration,
                baseType: ValueType.XsDuration,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            // dayTimeDuration (TODO: implement pattern)
            new(
                variety: Variety.Derived,
                name: ValueType.XsDayTimeDuration,
                baseType: ValueType.XsDuration,
                restrictions: new TypeRestrictions(
                    whitespace: WhitespaceHandling.Collapse // fixed
                )
            ),

            new(
                variety: Variety.Derived,
                name: ValueType.Function,
                baseType: ValueType.Item
            ),

            new(
                variety: Variety.Union,
                name: ValueType.XsError,
                memberTypes: Array.Empty<ValueType>()
            ),

            new(
                variety: Variety.Derived,
                name: ValueType.Map,
                baseType: ValueType.Function
            ),

            new(
                variety: Variety.Derived,
                name: ValueType.Array,
                baseType: ValueType.Function
            ),

            new(
                variety: Variety.Primitive,
                name: ValueType.Node,
                parentType: ValueType.Item
            ),

            new(
                variety: Variety.Derived,
                name: ValueType.Element,
                baseType: ValueType.Node
            ),

            new(
                variety: Variety.Derived,
                name: ValueType.Comment,
                baseType: ValueType.Node
            ),

            new(
                variety: Variety.Derived,
                name: ValueType.Attribute,
                baseType: ValueType.Node
            ),

            new(
                variety: Variety.Derived,
                name: ValueType.Text,
                baseType: ValueType.Node
            ),

            new(
                variety: Variety.Derived,
                name: ValueType.ProcessingInstruction,
                baseType: ValueType.Node
            ),

            new(
                variety: Variety.Derived,
                name: ValueType.DocumentNode,
                baseType: ValueType.Node
            ),

            new(
                variety: Variety.Union,
                name: ValueType.XsNumeric,
                memberTypes: new[]
                {
                    ValueType.XsDecimal,
                    ValueType.XsInteger,
                    ValueType.XsFloat,
                    ValueType.XsDouble
                }
            ),

            new(
                variety: Variety.Union,
                name: ValueType.None,
                memberTypes: Array.Empty<ValueType>()
            )
        };
    }
    // Resharper restore all

    public static BuiltInTypeModels Instance { get; } = new();

    public IEnumerable<BuiltInModelTypeDeclaration> BuiltinModels => _builtinModels;
}