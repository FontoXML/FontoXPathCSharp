using System.Text.RegularExpressions;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsString<TNode>
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

    public static readonly FunctionSignature<ISequence, TNode> FnString = (_, executionParameters, _, sequences) =>
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

    private static readonly FunctionSignature<ISequence, TNode> FnCompare = (_, _, _, args) =>
    {
        var arg1 = args[0];
        var arg2 = args[1];

        if (arg1.IsEmpty() || arg2.IsEmpty()) return SequenceFactory.CreateEmpty();

        var arg1Value = arg1.First()?.GetAs<StringValue>().Value;
        var arg2Value = arg2.First()?.GetAs<StringValue>().Value;

        return SequenceFactory.CreateFromValue(
            AtomicValue.Create(
                string.Compare(arg1Value, arg2Value, StringComparison.Ordinal),
                ValueType.XsInteger
            )
        );
    };

    private static readonly FunctionSignature<ISequence, TNode> CollationError = (_, _, _, _) =>
        throw new Exception("FOCH0002: No collations are supported");

    private static readonly FunctionSignature<ISequence, TNode> FnContains = (_, _, _, args) =>
    {
        var arg1 = args[0];
        var arg2 = args[1];

        var contains = !arg2.IsEmpty() ? arg2.First()?.GetAs<StringValue>().Value : "";
        if (string.IsNullOrEmpty(contains)) return SequenceFactory.SingletonTrueSequence;

        var stringToTest = !arg1.IsEmpty() ? arg1.First()?.GetAs<StringValue>().Value : "";
        if (string.IsNullOrEmpty(stringToTest)) return SequenceFactory.SingletonFalseSequence;

        // TODO: choose a collation, this defines whether eszett (ß) should equal 'ss'
        if (stringToTest.Contains(contains)) return SequenceFactory.SingletonTrueSequence;
        return SequenceFactory.SingletonFalseSequence;
    };

    private static readonly FunctionSignature<ISequence, TNode> FnEndsWith = (_, _, _, args) =>
    {
        var arg1 = args[0];
        var arg2 = args[1];

        var endsWith = !arg2.IsEmpty() ? arg2.First()?.GetAs<StringValue>().Value : "";
        if (string.IsNullOrEmpty(endsWith)) return SequenceFactory.SingletonTrueSequence;

        var stringToTest = !arg1.IsEmpty() ? arg1.First()?.GetAs<StringValue>().Value : "";
        if (string.IsNullOrEmpty(stringToTest)) return SequenceFactory.SingletonFalseSequence;

        // TODO: choose a collation, this defines whether eszett (ß) should equal 'ss'
        if (stringToTest.EndsWith(endsWith)) return SequenceFactory.SingletonTrueSequence;
        return SequenceFactory.SingletonFalseSequence;
    };

    private static readonly FunctionSignature<ISequence, TNode> FnStartsWith = (_, _, _, args) =>
    {
        var arg1 = args[0];
        var arg2 = args[1];

        var startsWith = !arg2.IsEmpty() ? arg2.First()?.GetAs<StringValue>().Value : "";
        if (string.IsNullOrEmpty(startsWith)) return SequenceFactory.SingletonTrueSequence;

        var stringToTest = !arg1.IsEmpty() ? arg1.First()?.GetAs<StringValue>().Value : "";
        if (string.IsNullOrEmpty(stringToTest)) return SequenceFactory.SingletonFalseSequence;

        // TODO: choose a collation, this defines whether eszett (ß) should equal 'ss'
        if (stringToTest.StartsWith(startsWith)) return SequenceFactory.SingletonTrueSequence;
        return SequenceFactory.SingletonFalseSequence;
    };

    private static readonly FunctionSignature<ISequence, TNode> FnSubstringBefore = (_, _, _, args) =>
    {
        var arg1 = args[0];
        var arg2 = args[1];

        var strArg2 = arg2.IsEmpty() ? "" : arg2.First()?.GetAs<StringValue>().Value ?? "";
        if (strArg2 == "") return SequenceFactory.CreateFromValue(AtomicValue.Create("", ValueType.XsString));

        var strArg1 = arg1.IsEmpty() ? "" : arg1.First()?.GetAs<StringValue>().Value ?? "";
        var startIndex = strArg1.IndexOf(strArg2, StringComparison.Ordinal);

        return SequenceFactory.CreateFromValue(startIndex == -1
            ? AtomicValue.Create("", ValueType.XsString)
            : AtomicValue.Create(strArg1[..startIndex], ValueType.XsString)
        );
    };

    private static readonly FunctionSignature<ISequence, TNode> FnSubstringAfter = (_, _, _, args) =>
    {
        var arg1 = args[0];
        var arg2 = args[1];

        var strArg1 = arg1.IsEmpty() ? "" : arg1.First()?.GetAs<StringValue>().Value ?? "";
        var strArg2 = arg2.IsEmpty() ? "" : arg2.First()?.GetAs<StringValue>().Value ?? "";

        if (strArg2 == "") return SequenceFactory.CreateFromValue(AtomicValue.Create(strArg1, ValueType.XsString));

        var startIndex = strArg1.IndexOf(strArg2, StringComparison.Ordinal);
        return SequenceFactory.CreateFromValue(startIndex == -1
            ? AtomicValue.Create("", ValueType.XsString)
            : AtomicValue.Create(strArg1[(startIndex + strArg2.Length)..], ValueType.XsString)
        );
    };

    private static readonly FunctionSignature<ISequence, TNode> FnSubstring = (_, _, _, args) =>
    {
        var sourceString = args[0];
        var start = args[1];
        var length = args.Length > 2 ? args[2] : null;

        var roundedStart = BuiltInFunctionsNumeric<TNode>.FnRound(
            false,
            start,
            null
        );
        var roundedLength =
            length != null
                ? BuiltInFunctionsNumeric<TNode>.FnRound(false, length, null)
                : null;

        var done = false;
        AbstractValue? sourceStringItem = null;
        AbstractValue? startItem = null;
        AbstractValue? lengthItem = null;
        return SequenceFactory.CreateFromIterator<AbstractValue>(_ =>
            {
                if (done) return IteratorResult<AbstractValue>.Done();
                if (sourceStringItem == null)
                {
                    sourceStringItem = sourceString.First();

                    if (sourceStringItem == null)
                    {
                        // The first argument can be the empty sequence
                        done = true;
                        return IteratorResult<AbstractValue>.Ready(AtomicValue.Create("", ValueType.XsString));
                    }
                }

                if (startItem == null) startItem = roundedStart.First();

                if (lengthItem == null && length != null)
                {
                    lengthItem = null;
                    lengthItem = roundedLength?.First();
                }

                done = true;

                var strValue = Convert.ToString(sourceStringItem.GetAs<AtomicValue>().GetValue()) ?? "";
                var startItemIndex = Convert.ToInt32(startItem?.GetAs<AtomicValue>().GetValue());
                var lengthIndex = Convert.ToInt32(lengthItem?.GetAs<AtomicValue>().GetValue());

                var slicedString = length != null
                    ? strValue[Math.Max(startItemIndex - 1, 0)..(startItemIndex + lengthIndex - 1)]
                    : strValue[Math.Max(startItemIndex - 1, 0)..];

                return IteratorResult<AbstractValue>.Ready(AtomicValue.Create(
                    slicedString,
                    ValueType.XsString
                ));
            }
        );
    };


    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnCompare,
            "compare",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ZeroOrOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            CollationError,
            "compare",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ZeroOrOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne),
                ParameterType.Ellipsis
            },
            FnConcat,
            "concat",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            CollationError,
            "contains",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnContains,
            "contains",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnEndsWith,
            "ends-with",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            CollationError,
            "ends-with",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnStartsWith,
            "starts-with",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            CollationError,
            "starts-with",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrOne)
            },
            FnString,
            "string",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnString),
            "string",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnSubstringBefore,
            "substring-before",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnSubstringAfter,
            "substring-after",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsDouble, SequenceMultiplicity.ExactlyOne)
            },
            FnSubstring,
            "substring",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(
            new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsDouble, SequenceMultiplicity.ExactlyOne),
                new ParameterType(ValueType.XsDouble, SequenceMultiplicity.ExactlyOne)
            },
            FnSubstring,
            "substring",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),
        
        new(new[]
            {
                new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)
            },
            FnStringLength,
            "string-length",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)
        ),

        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnStringLength),
            "string-length",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnNormalizeSpace,
            "normalize-space",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        // TODO: this is implemented differently in the javascript version
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnNormalizeSpace),
            "normalize-space",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            FnMatches,
            "matches",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        )
    };
}