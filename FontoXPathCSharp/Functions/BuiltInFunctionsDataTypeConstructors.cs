using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public class BuiltInFunctionsDataTypeConstructors<TNode> where TNode : notnull
{
    private static readonly FunctionSignature<ISequence, TNode> FnXsQName = (_, _, staticContext, args) =>
    {
        var sequence = args[0];

        if (sequence.IsEmpty()) return sequence;

        var value = sequence.First()!;
        if (value.GetValueType().IsSubtypeOf(ValueType.XsNumeric))
            // This won't ever work
            throw new XPathException("XPTY0004", "The provided QName is not a string-like value.");

        var lexicalQName = value.CastToType(ValueType.XsString).GetAs<StringValue>().Value;
        // Test lexical scope
        lexicalQName = TypeHelpers.NormalizeWhitespace(lexicalQName, ValueType.XsQName);
        if (!TypeHelpers.ValidatePattern(lexicalQName, ValueType.XsQName))
            throw new XPathException("FORG0001", "The provided QName is invalid.");

        if (!lexicalQName.Contains(':'))
        {
            // Only a local part
            var resolvedDefaultNamespaceUri = staticContext?.ResolveNamespace("");
            return SequenceFactory.CreateFromValue(
                AtomicValue.Create(
                    new QName("", resolvedDefaultNamespaceUri, lexicalQName),
                    ValueType.XsName
                )
            );
        }

        var prefixLocalName = lexicalQName.Split(':');
        var prefix = prefixLocalName[0];
        var localName = prefixLocalName[1];

        var namespaceUri = staticContext?.ResolveNamespace(prefix);
        if (namespaceUri == null)
            throw new XPathException(
                "FONS0004",
                $"The value {lexicalQName} can not be cast to a QName. Did you mean to use fn:QName?");

        return SequenceFactory.CreateFromValue(
            AtomicValue.Create(new QName(prefix, namespaceUri, localName), ValueType.XsQName)
        );
    };

    public static readonly BuiltinDeclarationType<TNode>[] Declarations = GenerateDeclarations();

    private static ISequence GenericDataTypeConstructor(
        ValueType dataType,
        ISequence sequence
    )
    {
        return sequence.IsEmpty()
            ? sequence
            : SequenceFactory.CreateFromValue(sequence.First()!.CastToType(dataType));
    }

    private static BuiltinDeclarationType<TNode>[] GenerateDeclarations()
    {
        var ZeroOrOneConstructorTypes = new Dictionary<string, ValueType>
        {
            { "untypedAtomic", ValueType.XsUntypedAtomic },
            { "error", ValueType.XsError },
            { "string", ValueType.XsString },
            { "boolean", ValueType.XsBoolean },
            { "decimal", ValueType.XsDecimal },
            { "float", ValueType.XsFloat },
            { "double", ValueType.XsDouble },
            { "duration", ValueType.XsDuration },
            { "dateTime", ValueType.XsDateTime },
            { "dateTimeStamp", ValueType.XsDateTimeStamp },
            { "time", ValueType.XsTime },
            { "date", ValueType.XsDate },
            { "gYearMonth", ValueType.XsGYearMonth },
            { "gYear", ValueType.XsGYear },
            { "gMonthDay", ValueType.XsGMonthDay },
            { "gDay", ValueType.XsGDay },
            { "gMonth", ValueType.XsGMonth },
            { "hexBinary", ValueType.XsHexBinary },
            { "base64Binary", ValueType.XsBase64Binary },
            { "anyURI", ValueType.XsAnyUri },
            { "normalizedString", ValueType.XsNormalizedString },
            { "token", ValueType.XsToken },
            { "language", ValueType.XsLanguage },
            { "NMTOKEN", ValueType.XsNmToken },
            { "Name", ValueType.XsName },
            { "NCName", ValueType.XsNcName },
            { "ID", ValueType.XsId },
            { "IDREF", ValueType.XsIdRef },
            { "ENTITY", ValueType.XsEntity },
            { "integer", ValueType.XsInteger },
            { "nonPositiveInteger", ValueType.XsNonPositiveInteger },
            { "negativeInteger", ValueType.XsNegativeInteger },
            { "long", ValueType.XsLong },
            { "int", ValueType.XsInt },
            { "short", ValueType.XsShort },
            { "byte", ValueType.XsByte },
            { "nonNegativeInteger", ValueType.XsNonNegativeInteger },
            { "unsignedLong", ValueType.XsUnsignedLong },
            { "unsignedInt", ValueType.XsUnsignedInt },
            { "unsignedShort", ValueType.XsUnsignedShort },
            { "unsignedByte", ValueType.XsUnsignedByte },
            { "positiveInteger", ValueType.XsPositiveInteger },
            { "yearMonthDuration", ValueType.XsYearMonthDuration },
            { "dayTimeDuration", ValueType.XsDayTimeDuration }
        };

        var zeroOrOneDeclarations = ZeroOrOneConstructorTypes
            .Select(nameValueType => new BuiltinDeclarationType<TNode>(
                new[] { new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne) },
                (_, _, _, args) => GenericDataTypeConstructor(nameValueType.Value, args[0]),
                nameValueType.Key,
                BuiltInUri.XMLSCHEMA_NAMESPACE_URI.GetBuiltinNamespaceUri(),
                new SequenceType(nameValueType.Value, SequenceMultiplicity.ZeroOrOne))
            ).ToList();

        var zeroOrMoreConstructorTypes = new Dictionary<string, ValueType>
        {
            { "NMTOKENS", ValueType.XsNmTokens },
            { "IDREFS", ValueType.XsIdRefs },
            { "ENTITIES", ValueType.XsEntities }
        };

        var zeroOrMoredeclarations = zeroOrMoreConstructorTypes
            .Select(nameValueType => new BuiltinDeclarationType<TNode>(
                new[] { new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne) },
                (_, _, _, args) => GenericDataTypeConstructor(nameValueType.Value, args[0]),
                nameValueType.Key,
                BuiltInUri.XMLSCHEMA_NAMESPACE_URI.GetBuiltinNamespaceUri(),
                new SequenceType(nameValueType.Value, SequenceMultiplicity.ZeroOrMore))
            ).ToList();


        var qNameDeclaration = new[]
        {
            new BuiltinDeclarationType<TNode>(
                new[] { new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne) },
                FnXsQName,
                "QName",
                BuiltInUri.XMLSCHEMA_NAMESPACE_URI.GetBuiltinNamespaceUri(),
                new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrMore))
        };

        return zeroOrOneDeclarations.Concat(zeroOrMoredeclarations).Concat(qNameDeclaration).ToArray();
    }
}