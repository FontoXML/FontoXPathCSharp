using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsString
{
    private static readonly FunctionDefinitionType<ISequence> FnStringLength = (_, _, _, args) =>
    {
        if (args.Length == 0) return SequenceFactory.CreateFromValue(new IntValue(0));

        var stringValue = args[0].First()!.GetAs<StringValue>(ValueType.XsString)!.Value;

        return SequenceFactory.CreateFromValue(new IntValue(stringValue.Length));
    };

    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[] {new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)},
            FnStringLength, "string-length",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)),

        new(Array.Empty<ParameterType>(),
            BuiltInFunctions.ContextItemAsFirstArgument(FnStringLength), "string-length",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne))
    };
}