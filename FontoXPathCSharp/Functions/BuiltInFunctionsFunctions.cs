using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public class BuiltInFunctionsFunctions
{
    private static readonly FunctionSignature<ISequence> FnFunctionLookup = (_, _, staticContext, sequences) =>
    {
        return ISequence.ZipSingleton(sequences, nameArityTuple =>
        {
            var name = nameArityTuple[0]?.GetAs<QNameValue>();
            var arity = nameArityTuple[1]?.GetAs<IntValue>();

            var functionProperties = staticContext?.LookupFunction(
                name.Value.NamespaceUri,
                name.Value.LocalName,
                arity.Value
            );

            if (functionProperties == null) return SequenceFactory.CreateEmpty();

            var functionItem = new FunctionValue<ISequence>(
                functionProperties.ArgumentTypes,
                arity.Value,
                name.Value.LocalName,
                name.Value.NamespaceUri,
                functionProperties.ReturnType,
                functionProperties.CallFunction
            );

            return new SingletonSequence(functionItem);
        });
    };


    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[]
            {
                new ParameterType(ValueType.XsQName, SequenceMultiplicity.ExactlyOne),
                new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ZeroOrOne)
            },
            FnFunctionLookup, "function-lookup", BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Function, SequenceMultiplicity.ZeroOrOne))
    };
}