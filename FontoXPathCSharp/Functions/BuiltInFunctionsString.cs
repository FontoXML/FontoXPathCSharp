using System.Text.RegularExpressions;
using System.Web;
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
            Atomize.AtomizeSequence(sequence, executionParameters).MapAll(allValues =>
                SequenceFactory.CreateFromValue(AtomicValue.Create(
                    string.Join("", allValues.Select(x => x.GetAs<AtomicValue>().GetValue())),
                    ValueType.XsString))));

        return ISequence.ZipSingleton(stringSequences,
            stringValues =>
                SequenceFactory.CreateFromValue(AtomicValue.Create(string.Join("", stringValues.Select(x =>
                        x.GetAs<StringValue>().Value)),
                    ValueType.XsString)));
    };

    private static readonly FunctionSignature<ISequence, TNode> FnStringLength = (_, _, _, args) =>
    {
        if (args.Length == 0) return SequenceFactory.CreateFromValue(new IntValue(0));

        var stringValue = args[0].First()!.GetAs<StringValue>().Value;

        return SequenceFactory.CreateFromValue(new IntValue(stringValue.Length));
    };

    private static readonly FunctionSignature<ISequence, TNode> FnNormalizeSpace = (_, _, _, args) =>
    {
        if (args.Length == 0) return SequenceFactory.CreateFromValue(new StringValue(""));

        var stringValue = args[0].First()!.GetAs<StringValue>().Value.Trim();
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

    // ReSharper disable once StaticMemberInGenericType
    // ReSharper disable once CollectionNeverUpdated.Local
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
        throw new XPathException("FOCH0002", "No collations are supported");

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


    private static readonly FunctionSignature<ISequence, TNode> FnUpperCase = (_, _, _, args) =>
    {
        var stringSequence = args[0];

        return stringSequence.IsEmpty()
            ? SequenceFactory.CreateFromValue(AtomicValue.Create("", ValueType.XsString))
            : stringSequence.Map((stringValue, _, _) =>
                AtomicValue.Create(stringValue.GetAs<StringValue>().Value.ToUpper(), ValueType.XsString));
    };

    private static readonly FunctionSignature<ISequence, TNode> FnLowerCase = (_, _, _, args) =>
    {
        var stringSequence = args[0];

        return stringSequence.IsEmpty()
            ? SequenceFactory.CreateFromValue(AtomicValue.Create("", ValueType.XsString))
            : stringSequence.Map((stringValue, _, _) =>
                AtomicValue.Create(stringValue.GetAs<StringValue>().Value.ToLower(), ValueType.XsString));
    };

    private static readonly FunctionSignature<ISequence, TNode> FnStringJoin = (_, executionParameters, _, args) =>
    {
        var sequence = args[0];
        var separator = args[1];

        return ISequence.ZipSingleton(new[] { separator }, separatorList =>
            {
                var separatorString = separatorList.First()?.GetAs<StringValue>().Value;
                return Atomize.AtomizeSequence(sequence, executionParameters).MapAll(allStrings =>
                {
                    var joinedString = string.Join(separatorString, allStrings.Select(stringValue =>
                        stringValue.CastToType(ValueType.XsString).GetAs<StringValue>().Value));
                    return SequenceFactory.CreateFromValue(AtomicValue.Create(joinedString, ValueType.XsString));
                });
            }
        );
    };

    private static readonly Dictionary<string, Regex> CompiledRegexes = new();

    private static readonly FunctionSignature<ISequence, TNode> FnTokenize = (_, _, _, args) =>
    {
        var input = args[0];
        var pattern = args[1];

        if (input.IsEmpty() || input.First()!.GetAs<StringValue>().Value.Length == 0)
            return SequenceFactory.CreateEmpty();

        var inputString = input.First()!.GetAs<StringValue>().Value;
        var patternString = pattern.First()!.GetAs<StringValue>().Value;

        var regex = CompileJsRegex(patternString);

        return SequenceFactory.CreateFromArray(
            regex.Matches(inputString)
                .Select(token => AtomicValue.Create(token.Value, ValueType.XsString) as AbstractValue)
                .ToArray()
        );
    };

    private static readonly FunctionSignature<ISequence, TNode> FnTranslate = (_, _, _, args) =>
    {
        var argSequence = args[0];
        var mapStringSequence = args[1];
        var transStringSequence = args[2];

        return ISequence.ZipSingleton(
            new[] { argSequence, mapStringSequence, transStringSequence },
            sequences =>
            {
                var argValue = sequences[0];
                var mapStringSequenceValue = sequences[1];
                var transStringSequenceValue = sequences[2];

                var argArr = (argValue != null ? argValue.GetAs<StringValue>().Value : "").ToCharArray();
                var mapString = mapStringSequenceValue?.GetAs<StringValue>().Value ?? "";
                var transStringArr =
                    (transStringSequenceValue != null ? transStringSequenceValue.GetAs<StringValue>().Value : "")
                    .ToCharArray();

                var result = argArr.Select(letter =>
                {
                    if (mapString.Contains(letter))
                    {
                        var index = mapString.IndexOf(letter);
                        if (index <= transStringArr.Length) return transStringArr[index];
                    }

                    return letter;
                });
                return SequenceFactory.CreateFromValue(
                    AtomicValue.Create(string.Join("", result), ValueType.XsString)
                );
            }
        );
    };

    private static readonly FunctionSignature<ISequence, TNode> FnCodepointsToString = (_, _, _, args) =>
    {
        var numberSequence = args[0];
        return numberSequence.MapAll(numbers =>
        {
            var str = string.Join("", numbers
                .Select(num =>
                {
                    var numericValue = num.GetAs<IntValue>().Value;
                    return numericValue is 0x9 or 0xa or 0xd
                        or >= 0x20 and <= 0xd7ff
                        or >= 0xe000 and <= 0xfffd
                        or >= 0x10000 and <= 0x10ffff
                        ? char.ConvertFromUtf32(numericValue)
                        : throw new XPathException("FOCH0001", 
                            "Could not convert codepoint to string, it is outside of the valid range.");
                }));
            return SequenceFactory.CreateFromValue(AtomicValue.Create(str, ValueType.XsString));
        });
    };

    private static readonly FunctionSignature<ISequence, TNode> FnStringToCodepoints = (_, _, _, args) =>
    {
        return ISequence.ZipSingleton(args, stringSequence =>
        {
            var str = stringSequence[0];
            var characters = str != null ? str.GetAs<StringValue>().Value : "";
            if (characters.Length == 0) return SequenceFactory.CreateEmpty();

            // C# and JS handle strings differently, so this is needed to do the same thing in C#
            var codePoints = new List<AbstractValue>();
            for (var i = 0; i < characters.Length; i += char.IsSurrogatePair(characters, i) ? 2 : 1)
            {
                var codepoint = char.ConvertToUtf32(characters, i);
                codePoints.Add(AtomicValue.Create(codepoint, ValueType.XsInteger));
            }

            return SequenceFactory.CreateFromArray(codePoints.ToArray());
        });
    };

    private static readonly FunctionSignature<ISequence, TNode> FnEncodeForUri = (_, _, _, stringSequence) =>
    {
        return ISequence.ZipSingleton(stringSequence, stringSequenceVal =>
        {
            var str = stringSequenceVal.FirstOrDefault();
            if (str == null || str.GetAs<StringValue>().Value.Length == 0)
                return SequenceFactory.CreateFromValue(AtomicValue.Create("", ValueType.XsString));

            // Adhering RFC 3986 which reserves !, ', (, ), and *
            var regex = new Regex("[!'()*]");
            var encoded = regex.Replace(HttpUtility.UrlEncode(
                    str.GetAs<StringValue>().Value),
                c => '%' + Convert.ToString(c.Value[0], 16).ToUpper()
            );
            return SequenceFactory.CreateFromValue(AtomicValue.Create(
                encoded,
                ValueType.XsString
            ));
        });
    };

    private static readonly FunctionSignature<ISequence, TNode> FnIriToUri = (_, _, _, stringSequence) =>
    {
        return ISequence.ZipSingleton(stringSequence, stringSequenceVal =>
        {
            var str = stringSequenceVal.FirstOrDefault();
            if (str == null || str.GetAs<StringValue>().Value.Length == 0)
                return SequenceFactory.CreateFromValue(AtomicValue.Create("", ValueType.XsString));


            var strValue = str.GetAs<StringValue>().Value;

            return SequenceFactory.CreateFromValue(
                AtomicValue.Create(
                    Regex.Replace(
                        strValue,
                        @"([\u00A0-\uD7FF\uE000-\uFDCF\uFDF0-\uFFEF ""<>{}|\\^`/\n\u007f\u0080-\u009f]|[\uD800-\uDBFF][\uDC00-\uDFFF])",
                        match => HttpUtility.UrlEncode(match.Value)
                    ),
                    ValueType.XsString
                )
            );
        });
    };

    private static readonly FunctionSignature<ISequence, TNode> FnCodepointEqual = (_, _, _, args) =>
    {
        var stringSequence1 = args[0];
        var stringSequence2 = args[1];
        return ISequence.ZipSingleton(new[] { stringSequence1, stringSequence2 }, sequenceValues =>
        {
            var value1 = sequenceValues[0];
            var value2 = sequenceValues[1];
            if (value1 == null || value2 == null) return SequenceFactory.CreateEmpty();

            var string1 = value1.GetAs<StringValue>().Value;
            var string2 = value2.GetAs<StringValue>().Value;

            if (string1.Length != string2.Length) return SequenceFactory.SingletonFalseSequence;

            for (var i = 0; i < string1.Length; i += char.IsSurrogatePair(string2, i) ? 2 : 1)
            {
                var codepoint1 = char.ConvertToUtf32(string1, i);
                var codepoint2 = char.ConvertToUtf32(string2, i);
                if (codepoint1 != codepoint2) return SequenceFactory.SingletonFalseSequence;
            }

            return SequenceFactory.SingletonTrueSequence;
        });
    };

    private static readonly FunctionSignature<ISequence, TNode> FnReplace = (_, _, _, args) =>
    {
        var inputSequence = args[0];
        var patternSequence = args[1];
        var replacementSequence = args[2];

        return ISequence.ZipSingleton(
            new[] { inputSequence, patternSequence, replacementSequence }, sequenceValues =>
            {
                var inputValue = sequenceValues[0];
                var patternValue = sequenceValues[1];
                var replacementValue = sequenceValues[2];
                var input = inputValue != null ? inputValue.GetAs<StringValue>().Value : "";
                var pattern = patternValue != null ? patternValue.GetAs<StringValue>().Value : "";
                var replacement = replacementValue != null ? replacementValue.GetAs<StringValue>().Value : "";
                if (replacement.Contains("$0"))
                    throw new Exception(
                        "Using $0 in fn:replace to replace substrings with full matches is not supported."
                    );

                // Note: while XPath patterns escape dollars with backslashes, JavaScript escapes them by duplicating
                replacement = string.Join("", Regex.Split(replacement, @"((?:\$\$)|(?:\\\$)|(?:\\\\))")
                    .Select(part =>
                    {
                        switch (part)
                        {
                            case "\\$":
                                return "$$";
                            case "\\\\":
                                return "\\";
                            case "$$":
                                throw new XPathException("FORX0004", "Invalid replacement: '$$'");
                            default:
                                return part;
                        }
                    }));

                // TODO: This is a bit silly, the re-stringification of the regex should not be needed. 
                var regex = CompileJsRegex(pattern);
                var result = Regex.Replace(input, regex.ToString(), replacement);

                return SequenceFactory.CreateFromValue(AtomicValue.Create(result, ValueType.XsString));
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
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnUpperCase,
            "upper-case",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnLowerCase,
            "lower-case",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            FnStringJoin,
            "string-join",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore)
            },
            (dynamicContext, executionParameters, staticContext, args) =>
                FnStringJoin(dynamicContext, executionParameters, staticContext, args[0],
                    SequenceFactory.CreateFromValue(AtomicValue.Create("", ValueType.XsString))),
            "string-join",
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
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            (_, _, _, _) => throw new NotImplementedException("Using flags in 'tokenize' is not supported"),
            "tokenize",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ZeroOrMore)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            FnTokenize,
            "tokenize",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ZeroOrMore)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            FnTranslate,
            "translate",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ZeroOrMore)
            },
            FnCodepointsToString,
            "codepoints-to-string",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrMore)
            },
            FnStringToCodepoints,
            "string-to-codepoints",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ZeroOrMore)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnEncodeForUri,
            "encode-for-uri",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnIriToUri,
            "iri-to-uri",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
            },
            FnCodepointEqual,
            "codepoint-equal",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ZeroOrOne)
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
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            FnReplace,
            "replace",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            (_, _, _, _) => throw new NotImplementedException("Using flags in 'replace' is not supported"),
            "replace",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        )
    };

    private static Regex CompileJsRegex(string pattern)
    {
        if (CompiledRegexes.ContainsKey(pattern)) return CompiledRegexes[pattern];

        Regex regex;
        try
        {
            regex = new Regex(pattern, RegexOptions.ECMAScript);
        }
        catch (Exception ex)
        {
            throw new XPathException("FORX0002", ex.Message);
        }

        // Only do this check once per regex
        if (regex.IsMatch(""))
            throw new XPathException("FORX0003", $"the pattern {pattern} matches the zero length string");

        CompiledRegexes.Add(pattern, regex);
        return regex;
    }
}