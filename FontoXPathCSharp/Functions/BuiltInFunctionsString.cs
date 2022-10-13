using System.Text.RegularExpressions;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsString<TNode> where TNode : notnull
{
    private static readonly FunctionSignature<ISequence, TNode> FnConcat = (_, executionParameters, _, args) =>
    {
        var stringSequences = args.Select(sequence =>
            Atomize.AtomizeSequence(sequence, executionParameters!).MapAll(allValues =>
                SequenceFactory.CreateFromValue(AtomicValue.Create(
                    string.Join("", allValues.Select(x => x.GetAs<AtomicValue>().GetValue())),
                    ValueType.XsString))));

        Console.WriteLine(stringSequences);

        return ISequence.ZipSingleton(stringSequences,
            stringValues =>
                SequenceFactory.CreateFromValue(AtomicValue.Create(string.Join("", stringValues.Select(x =>
                        x.GetAs<StringValue>().Value)),
                    ValueType.XsString)));
    };

    private static readonly FunctionSignature<ISequence, TNode> FnStringLength = (_, _, _, args) =>
    {
        if (args.Length == 0) return SequenceFactory.CreateFromValue(new IntValue(0));

        var stringValue = args[0].First()!.GetAs<StringValue>()!.Value;

        return SequenceFactory.CreateFromValue(new IntValue(stringValue.Length));
    };

    private static readonly FunctionSignature<ISequence, TNode> FnNormalizeSpace = (_, _, _, args) =>
    {
        if (args.Length == 0) return SequenceFactory.CreateFromValue(new StringValue(""));

        var stringValue = args[0].First()!.GetAs<StringValue>()!.Value.Trim();
        return SequenceFactory.CreateFromValue(new StringValue(Regex.Replace(stringValue, @"\s+", " ")));
    };

    private static readonly FunctionSignature<ISequence, TNode> FnString = (_, executionParameters, _, sequences) =>
    {
        var sequence = sequences[0];
        if (sequence.IsEmpty()) return SequenceFactory.CreateFromValue(AtomicValue.Create("", ValueType.XsString));
        return sequence.Map((value, _, _) =>
        {
            if (value.GetValueType().IsSubtypeOf(ValueType.Node))
            {
                var stringValueSequence = Atomize.AtomizeSingleValue(value, executionParameters);
                var stringValue = stringValueSequence.First();

                return value.GetValueType().IsSubtypeOf(ValueType.Attribute)
                    ? stringValue!.CastToType(ValueType.XsString)
                    : stringValue!;
            }

            return value.CastToType(ValueType.XsString);
        });
    };

    private static readonly Dictionary<string, Func<string, bool>> CachedPatterns = new();

    private static readonly FunctionSignature<ISequence, TNode> FnMatches = (_, _, _, sequences) =>
    {
        return ISequence.ZipSingleton(sequences, sequenceValues =>
        {
            var input = Convert.ToString(sequenceValues[0].GetAs<AtomicValue>().GetValue()) ?? "";
            var pattern = Convert.ToString(sequenceValues[1].GetAs<AtomicValue>().GetValue()) ?? "";

            Func<string, bool> compiledPattern = _ => false;
            if (!CachedPatterns.ContainsKey(pattern))
                throw new NotImplementedException(
                    "BuiltInFunctionsString.FnMatches, XSD pattern compiling needs to be added.");

            return compiledPattern(input)
                ? SequenceFactory.SingletonTrueSequence
                : SequenceFactory.SingletonFalseSequence;
        });
    };

    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(
            new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne),
                ParameterType.Ellipsis
            },
            FnConcat, "concat",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)),
        new(new[] { new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne) },
            FnStringLength, "string-length",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)),
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnStringLength), "string-length",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)),

        new(new[] { new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne) },
            FnNormalizeSpace, "normalize-space",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)),
        // TODO: this is implemented differently in the javascript version
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnNormalizeSpace), "normalize-space",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)),
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrOne) },
            FnString,
            "string",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)),
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnString),
            "string",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)),
        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            FnMatches,
            "matches",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne))
    };
}