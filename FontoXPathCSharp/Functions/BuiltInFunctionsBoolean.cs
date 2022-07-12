using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsBoolean
{
    private static readonly FunctionSignature<ISequence> FnNot = (_, _, _, args) =>
        SequenceFactory.CreateFromValue(new BooleanValue(!args[0].GetEffectiveBooleanValue()));

    private static readonly FunctionSignature<ISequence> FnBoolean = (_, _, _, args) =>
        SequenceFactory.CreateFromValue(new BooleanValue(args[0].GetEffectiveBooleanValue()));

    private static readonly FunctionSignature<ISequence> FnTrue = (_, _, _, _) =>
        SequenceFactory.CreateFromValue(new BooleanValue(true));

    private static readonly FunctionSignature<ISequence> FnFalse = (_, _, _, _) =>
        SequenceFactory.CreateFromValue(new BooleanValue(false));

    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore) }, FnBoolean, "boolean",
            BuiltInNamespaceUris.FunctionsNamespaceUri.GetUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)),
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore) }, FnNot, "not",
            BuiltInNamespaceUris.FunctionsNamespaceUri.GetUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)),
        new(Array.Empty<ParameterType>(), FnTrue, "true", BuiltInNamespaceUris.FunctionsNamespaceUri.GetUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)),
        new(Array.Empty<ParameterType>(), FnFalse, "false", BuiltInNamespaceUris.FunctionsNamespaceUri.GetUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne))
    };
}