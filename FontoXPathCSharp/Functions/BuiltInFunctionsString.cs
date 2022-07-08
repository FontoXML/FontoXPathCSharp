using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsString
{
    private static readonly FunctionDefinitionType<ISequence> FnConcat = (_, executionParameters, _, args) =>
    {
        var stringSequences = args.Select(sequence =>
            Atomize.AtomizeSequence(sequence, executionParameters!).MapAll(allValues =>
                    SequenceFactory.CreateFromValue(Atomize.CreateAtomicValue(
                        string.Join("", allValues.Select(x => x.GetAs<StringValue>()?.Value)),
                        ValueType.XsString)),
                IterationHint.None));

        Console.WriteLine(stringSequences);

        return ISequence.ZipSingleton(stringSequences,
            (stringValues) =>
                SequenceFactory.CreateFromValue(Atomize.CreateAtomicValue(string.Join("", stringValues.Select(x =>
                        x.GetAs<StringValue>().Value)),
                    ValueType.XsString)));
    };

    private static readonly FunctionDefinitionType<ISequence> FnStringLength = (_, _, _, args) =>
    {
        if (args.Length == 0) return SequenceFactory.CreateFromValue(new IntValue(0));

        var stringValue = args[0].First()!.GetAs<StringValue>()!.Value;

        return SequenceFactory.CreateFromValue(new IntValue(stringValue.Length));
    };

    private static readonly FunctionDefinitionType<ISequence> FnNormalizeSpace = (_, _, _, args) =>
    {
        if (args.Length == 0) return SequenceFactory.CreateFromValue(new StringValue(""));

        var stringValue = args[0].First()!.GetAs<StringValue>()!.Value.Trim();
        return SequenceFactory.CreateFromValue(new StringValue(Regex.Replace(stringValue, @"\s+", " ")));
    };

    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(
            new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne),
                ParameterType.Ellipsis()
            },
            FnConcat, "concat",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)),
        new(new[] {new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)},
            FnStringLength, "string-length",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)),
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions.ContextItemAsFirstArgument(FnStringLength), "string-length",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)),

        new(new[] {new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)},
            FnNormalizeSpace, "normalize-space",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)),
        // TODO: this is implemented differently in the javascript version
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions.ContextItemAsFirstArgument(FnNormalizeSpace), "normalize-space",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne))
    };
}