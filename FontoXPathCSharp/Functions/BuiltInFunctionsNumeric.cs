using System.Globalization;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public class BuiltInFunctionsNumeric<TNode>
{
    private static readonly FunctionSignature<ISequence, TNode> FnNumber = (_, executionParameters, _, sequences) =>
    {
        var sequence = sequences[0];
        var atomized = Atomize.AtomizeSequence(sequence, executionParameters);
        if (atomized.IsEmpty())
            return SequenceFactory.CreateFromValue(AtomicValue.Create(double.NaN, ValueType.XsDouble));
        return sequence.First()?.TryCastToType(ValueType.XsDouble) switch
        {
            SuccessResult<AtomicValue> result => SequenceFactory.CreateFromValue(result.Data),
            ErrorResult<AtomicValue> => SequenceFactory.CreateFromValue(AtomicValue.Create(double.NaN,
                ValueType.XsDouble)),
            _ => throw new ArgumentOutOfRangeException(
                $"BuiltInFunctionsNumeric: Unexpected parameter in fn:number: ({atomized.IsSingleton()}).")
        };
    };

    private static readonly FunctionSignature<ISequence, TNode> FnAbs = (_, _, _, sequences) =>
    {
        var sequence = sequences[0];
        return sequence.Map((onlyValue, _, _) =>
            CreateValidNumericType(onlyValue.GetValueType(),
                Math.Abs(Convert.ToDouble(onlyValue.GetAs<UntypedAtomicValue>())))
        );
    };

    private static readonly FunctionSignature<ISequence, TNode> FnCeiling = (_, _, _, sequences) =>
    {
        var sequence = sequences[0];
        return sequence.Map((onlyValue, _, _) =>
            CreateValidNumericType(onlyValue.GetValueType(),
                Math.Ceiling(Convert.ToDouble(onlyValue.GetAs<UntypedAtomicValue>())))
        );
    };

    private static readonly FunctionSignature<ISequence, TNode> FnFloor = (_, _, _, sequences) =>
    {
        var sequence = sequences[0];
        return sequence.Map((onlyValue, _, _) =>
            CreateValidNumericType(onlyValue.GetValueType(),
                Math.Floor(Convert.ToDouble(onlyValue.GetAs<UntypedAtomicValue>())))
        );
    };

    private static readonly FunctionSignature<ISequence, TNode> FnFormatInteger = (_, _, _, sequences) =>
    {
        var sequence = sequences[0];
        var pictureSequence = sequences[1];

        if (sequence.IsEmpty()) return SequenceFactory.CreateFromValue(AtomicValue.Create("", ValueType.XsString));

        var sequenceValue = sequence.First()!.GetAs<StringValue>();
        var pictureValue = pictureSequence.First()!.GetAs<StringValue>();

        switch (pictureValue.Value)
        {
            case "I":
                return SequenceFactory.CreateFromValue(AtomicValue.Create(
                    ConvertIntegerToRoman(sequenceValue.Value, false), ValueType.XsString));
            case "i":
                return SequenceFactory.CreateFromValue(AtomicValue.Create(
                    ConvertIntegerToRoman(sequenceValue.Value, true), ValueType.XsString));
            case "A":
                return SequenceFactory.CreateFromValue(AtomicValue.Create(
                    ConvertIntegerToAlphabet(sequenceValue.Value, false), ValueType.XsString));
            case "a":
                return SequenceFactory.CreateFromValue(AtomicValue.Create(
                    ConvertIntegerToAlphabet(sequenceValue.Value, true), ValueType.XsString));
            default:
                throw new Exception(
                    $"Picture: {pictureValue!.Value} is not implemented yet. The supported picture strings are 'A', 'a', 'I', and 'i'"
                );
        }
    };

    private static readonly char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Split().Select(s => s[0]).ToArray();

    private static readonly Dictionary<string, int> RomanNumbers = new()
    {
        { "M", 1000 },
        { "CM", 900 },
        { "D", 500 },
        { "CD", 400 },
        { "C", 100 },
        { "XC", 90 },
        { "L", 50 },
        { "XL", 40 },
        { "X", 10 },
        { "IX", 9 },
        { "V", 5 },
        { "IV", 4 },
        { "I", 1 }
    };

    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(new[]
            {
                new ParameterType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne)
            },
            FnAbs,
            "abs",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne)),

        new(new[]
            {
                new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            FnFormatInteger,
            "format-integer",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)),

        new(new[]
            {
                new ParameterType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne)
            },
            FnCeiling,
            "ceiling",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne)),

        new(new[]
            {
                new ParameterType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne)
            },
            FnFloor,
            "floor",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne)),

        new(new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne)
            },
            FnNumber,
            "number",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsDouble, SequenceMultiplicity.ExactlyOne)),
        new(Array.Empty<ParameterType>(),
            (dynamicContext, executionParameters, staticContext, _) =>
            {
                var atomizedContextItem = dynamicContext?.ContextItem == null
                    ? null
                    : ArgumentHelper.PerformFunctionConversion(
                        new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne),
                        SequenceFactory.CreateFromValue(dynamicContext.ContextItem), executionParameters, "fn:number",
                        false);
                if (atomizedContextItem == null)
                    throw new XPathException("XPDY0002", "fn:number needs an atomizable context item.");

                return FnNumber(dynamicContext, executionParameters, staticContext, atomizedContextItem);
            },
            "number",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsDouble, SequenceMultiplicity.ExactlyOne)),
        new(new[]
            {
                new ParameterType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne)
            },
            (_, _, _, param) => FnRound(false, param),
            "round",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne)),
        new(new[]
            {
                new ParameterType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)
            },
            (_, _, _, param) => FnRound(false, param), "round",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne)),
        new(new[] { new ParameterType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne) },
            (_, _, _, param) => FnRound(true, param), "round-half-to-even",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne)),
        new(new[]
            {
                new ParameterType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)
            },
            (_, _, _, param) => FnRound(true, param), "round-half-to-even",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsNumeric, SequenceMultiplicity.ZeroOrOne))
    };

    private static string ConvertIntegerToAlphabet(string intString, bool isLowerCase)
    {
        if (!int.TryParse(intString, out var integer)) return "-";

        var isNegative = integer < 0;

        integer = Math.Abs(integer);

        var output = "";

        while (integer > 0)
        {
            var digit = (integer - 1) % Alphabet.Length;
            output = Alphabet[digit] + output;
            integer = ((integer - digit) / Alphabet.Length) | 0;
        }

        if (isLowerCase) output = output.ToLower();

        return isNegative ? "-${output}" : output;
    }

    private static string ConvertIntegerToRoman(string intString, bool isLowerCase)
    {
        if (!int.TryParse(intString, out var integer)) return "-";

        var isNegative = integer < 0;

        integer = Math.Abs(integer);

        var romanString = RomanNumbers.Aggregate(
            "",
            (str, roman) =>
            {
                var q = (int)Math.Floor(integer / (double)roman.Value);
                integer -= q * roman.Value;
                return str + string.Concat(Enumerable.Repeat(roman.Key, q));
            });
        if (isLowerCase) romanString = romanString.ToLower();
        return isNegative ? $"-{romanString}" : romanString;
    }

    private static AbstractValue CreateValidNumericType(ValueType type, double transformedValue)
    {
        if (type.IsSubtypeOf(ValueType.XsInteger)) return AtomicValue.Create(transformedValue, ValueType.XsInteger);
        if (type.IsSubtypeOf(ValueType.XsFloat)) return AtomicValue.Create(transformedValue, ValueType.XsFloat);
        if (type.IsSubtypeOf(ValueType.XsDouble)) return AtomicValue.Create(transformedValue, ValueType.XsDouble);
        // It must be a decimal, only four numeric types
        return AtomicValue.Create(transformedValue, ValueType.XsDecimal);
    }

    private static ISequence FnRound(
        bool halfToEven,
        params ISequence[] sequences)
    {
        var sequence = sequences[0];
        var precision = sequences.Length > 1 ? sequences[1] : null;

        var done = false;
        return SequenceFactory.CreateFromIterator(
            _ =>
            {
                if (done) return IteratorResult<AbstractValue>.Done();

                if (sequence.IsEmpty())
                {
                    // Empty sequence
                    done = true;
                    return IteratorResult<AbstractValue>.Done();
                }

                var firstValue = sequence.First()!;
                var numericValue = Convert.ToDecimal(firstValue.GetAs<AtomicValue>().GetValue());

                if (firstValue.GetValueType().IsSubtypeOfAny(ValueType.XsFloat, ValueType.XsDouble))
                {
                    // TODO: Check if this is sufficient for floats as well, or if they need a separate pass.
                    var doubleValue = Convert.ToDouble(numericValue);
                    if (doubleValue == 0 || double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
                    {
                        done = true;
                        return IteratorResult<AbstractValue>.Ready(firstValue);
                    }
                }

                var scalingPrecision = precision != null
                    ? Convert.ToInt32(precision.First()?.GetAs<AtomicValue>().GetValue())
                    : 0;

                done = true;

                if (GetNumberOfDecimalDigits(numericValue) < scalingPrecision)
                    return IteratorResult<AbstractValue>.Ready(firstValue);

                var originalType = Array.Find(new[]
                {
                    ValueType.XsInteger,
                    ValueType.XsDecimal,
                    ValueType.XsDouble,
                    ValueType.XsFloat
                }, type => firstValue.GetValueType().IsSubtypeOf(type));

                var itemAsDecimal = firstValue.CastToType(ValueType.XsDecimal).GetAs<AtomicValue>();
                var scaling = Math.Pow(10, scalingPrecision);
                var roundedNumber = DetermineRoundedNumber(Convert.ToDecimal(itemAsDecimal.GetValue()), halfToEven,
                    (decimal)scaling);
                return originalType switch
                {
                    ValueType.XsDecimal => IteratorResult<AbstractValue>.Ready(AtomicValue.Create(roundedNumber,
                        ValueType.XsDecimal)),
                    ValueType.XsDouble => IteratorResult<AbstractValue>.Ready(AtomicValue.Create(roundedNumber,
                        ValueType.XsDouble)),
                    ValueType.XsFloat => IteratorResult<AbstractValue>.Ready(AtomicValue.Create(roundedNumber,
                        ValueType.XsFloat)),
                    ValueType.XsInteger => IteratorResult<AbstractValue>.Ready(AtomicValue.Create(roundedNumber,
                        ValueType.XsInteger)),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        );
    }

    private static decimal DetermineRoundedNumber(decimal itemAsDecimal, bool halfToEven, decimal scaling)
    {
        if (halfToEven && IsHalf(itemAsDecimal, scaling))
        {
            if (Math.Floor(itemAsDecimal * scaling) % 2 == 0) return Math.Floor(itemAsDecimal * scaling) / scaling;
            return Math.Ceiling(itemAsDecimal * scaling) / scaling;
        }

        return Math.Round(itemAsDecimal * scaling) / scaling;
    }

    private static bool IsHalf(decimal value, decimal scaling)
    {
        return value * scaling % 1 % 0.5m == 0;
    }

    private static int GetNumberOfDecimalDigits(decimal numericValue)
    {
        var stringRepresentation = numericValue.ToString(CultureInfo.InvariantCulture);
        if (!stringRepresentation.Contains('.')) return 0;
        var indexOfDecimal = stringRepresentation.IndexOf('.');
        return stringRepresentation.Length - indexOfDecimal - 1;
    }
}