using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public class BuiltInFunctionsQName
{
    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            FnQName, "QName",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsQName, SequenceMultiplicity.ExactlyOne))
    };

    private static readonly FunctionSignature<ISequence> FnQName = (_, _, _, param) =>
    {
        var paramUri = param[0];
        return SequenceUtils.ZipSingleton(param, values =>
        {
            var uriValue = values[0].GetAs<StringValue>();
            var lexicalQNameValue = values[1].GetAs<StringValue>();
            var lexicalQName = lexicalQNameValue.Value;
            if (!TypeHelpers.ValidatePattern(lexicalQName, ValueType.XsQName))
                throw new Exception("FOCA0002: The provided QName is invalid.");

            var uri = uriValue != null ? uriValue.Value : null;
            if (uri == null && lexicalQName.Contains(':'))
                throw new Exception("FOCA0002: The URI of a QName may not be empty if a prefix is provided.");
            // Skip URI validation for now

            if (paramUri.IsEmpty())
                return SequenceFactory.CreateFromValue(
                    Atomize.CreateAtomicValue(new QName("", null, lexicalQName), ValueType.XsQName)
                );

            if (!lexicalQName.Contains(':'))
                // Only a local part
                return SequenceFactory.CreateFromValue(
                    Atomize.CreateAtomicValue(new QName("", uri, lexicalQName), ValueType.XsQName)
                );

            var prefixLocalName = lexicalQName.Split(':');
            return SequenceFactory.CreateFromValue(
                Atomize.CreateAtomicValue(new QName(prefixLocalName[0], uri, prefixLocalName[1]), ValueType.XsQName)
            );
        });
    };
}