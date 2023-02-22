using System.Collections;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.DataTypes.Builtins;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.EvaluationUtils;

public class AdaptToValues<TNode> where TNode : notnull
{
    public static ISequence AdaptValueToSequence(
        DomFacade<TNode> domFacade,
        object? value,
        SequenceType? expectedType = null)
    {
        expectedType ??= new SequenceType(ValueType.Item, SequenceMultiplicity.ZeroOrOne);
        return SequenceFactory.CreateFromArray(AdaptValueToArrayOfXPathValues(domFacade, value, expectedType));
    }

    public static AbstractValue[] AdaptValueToArrayOfXPathValues(
        DomFacade<TNode> domFacade,
        object value,
        SequenceType expectedType)
    {
        switch (expectedType.Multiplicity)
        {
            case SequenceMultiplicity.ZeroOrOne:
            {
                var converted = AdaptValueToXPath(expectedType.ValueType, value, domFacade);
                return converted == null ? Array.Empty<AbstractValue>() : new[] { converted };
            }
            case SequenceMultiplicity.ZeroOrMore or SequenceMultiplicity.OneOrMore when value is not IEnumerable:
                throw new Exception(
                    $"The value {value} should be an array if it is to be converted to {expectedType}."
                );
            case SequenceMultiplicity.ZeroOrMore or SequenceMultiplicity.OneOrMore:
                return ((IEnumerable)value).Cast<object?>()
                    .Select(val => AdaptValueToXPath(expectedType.ValueType, val, domFacade))
                    .Where(val => val != null)
                    .Cast<AbstractValue>()
                    .ToArray();
            default:
                var adaptedValue = AdaptValueToXPath(expectedType.ValueType, value, domFacade);
                if (adaptedValue == null)
                    throw new Exception(
                        $"The value {value} should be a single entry if it is to be converted to {expectedType}."
                    );
                return new[] { adaptedValue };
        }
    }

    private static AbstractValue? AdaptValueToXPath(
        ValueType valueType,
        object? value,
        DomFacade<TNode> domFacade)
    {
        if (value == null) return null;

        switch (valueType)
        {
            case ValueType.XsBoolean:
                return (bool)value ? AtomicValue.TrueBoolean : AtomicValue.FalseBoolean;
            case ValueType.XsString:
                return AtomicValue.Create(value + "", ValueType.XsString);
            case ValueType.XsDouble:
            case ValueType.XsNumeric:
                CheckNumericType(value, ValueType.XsDouble);
                return AtomicValue.Create(Convert.ToDouble(value), ValueType.XsDouble);
            case ValueType.XsDecimal:
                CheckNumericType(value, valueType);
                return AtomicValue.Create(Convert.ToDecimal(value), ValueType.XsDecimal);
            case ValueType.XsInteger:
                CheckNumericType(value, valueType);
                return AtomicValue.Create(Convert.ToInt64(value), ValueType.XsInteger);
            case ValueType.XsFloat:
                CheckNumericType(value, valueType);
                return AtomicValue.Create(Convert.ToSingle(value), ValueType.XsFloat);
            case ValueType.XsDate:
            case ValueType.XsTime:
            case ValueType.XsDateTime:
            case ValueType.XsGYearMonth:
            case ValueType.XsGYear:
            case ValueType.XsGMonthDay:
            case ValueType.XsGMonth:
            case ValueType.XsGDay:
                if (value is not (DateTimeOffset or DateTime))
                    throw new Exception(
                        $"The JavaScript value {value} with type {value.GetType().Name} is not a valid type to be converted to an XPath {valueType.Name()}."
                    );

                var isoString = "";
                if (value is DateTimeOffset)
                    isoString = ((DateTimeOffset)value).ToString("O");
                else if (value is DateTime) isoString = ((DateTime)value).ToString("O");

                //Not sure if the ConvertToType is needed here because of how we construct DateTimeValues.
                return DateTimeValue.FromString(isoString, valueType).ConvertToType(valueType);
            case ValueType.Node:
            case ValueType.Attribute:
            case ValueType.DocumentNode:
            case ValueType.Element:
            case ValueType.Text:
            case ValueType.ProcessingInstruction:
            case ValueType.Comment:
                if (value is not TNode node)
                    throw new Exception(
                        $"The value {value} with type {value.GetType().Name} is not a valid type to be converted to an XPath {valueType}.");
                return new NodeValue<TNode>(node, domFacade);
            case ValueType.Item:
            case ValueType.Map:
                return AdaptSingleValue(value, domFacade);
            default:
                throw new Exception(
                    "Values of the type '{valueTypeToString(type)}' can not be adapted from JavaScript to equivalent XPath values.");
        }
    }

    private static AbstractValue? AdaptSingleValue(object? value, DomFacade<TNode> domFacade)
    {
        if (value == null) return null;

        if (IsInteger(value)) return IntegerValue.CreateIntegerValue((long)value, ValueType.XsInteger);
        return value switch
        {
            bool b => b ? AtomicValue.TrueBoolean : AtomicValue.FalseBoolean,
            string s => StringValue.CreateStringValue(s),
            float f => FloatValue.CreateFloatValue(f),
            double dbl => DoubleValue.CreateDoubleValue(dbl),
            decimal dml => DecimalValue.CreateDecimalValue(dml),
            TNode node => new NodeValue<TNode>(node, domFacade),
            IList list => new ArrayValue<TNode>(list.Cast<object?>()
                .Select(var => AdaptSingleValue(var, domFacade))
                .Select(adaptedValue =>
                    adaptedValue == null
                        ? SequenceFactory.CreateEmpty()
                        : SequenceFactory.CreateFromValue(adaptedValue))
                .Select(ISequence.CreateDoublyIterableSequence)
                .ToList()),
            IDictionary dict => new MapValue(dict.Keys.OfType<object>()
                .Select(key =>
                {
                    var adaptedValue = AdaptSingleValue(dict[key], domFacade);
                    var adaptedSequence = adaptedValue == null
                        ? SequenceFactory.CreateEmpty()
                        : SequenceFactory.CreateFromValue(adaptedValue);
                    return new KeyValuePair<AbstractValue, Func<ISequence>>(AtomicValue.Create(key, ValueType.XsString),
                        ISequence.CreateDoublyIterableSequence(adaptedSequence));
                })
                .ToList()),
            DateTimeOffset dateTimeOffset => DateTimeValue.FromString(dateTimeOffset.ToString("o"),
                ValueType.XsDateTime),
            DateTime dateTime => DateTimeValue.FromString(dateTime.ToString("o"), ValueType.XsDateTime),
            _ => throw new Exception(
                $"Value {value} of type '{value.GetType().Name}' is not adaptable to an XPath value.")
        };
    }

    private static bool IsInteger(object value)
    {
        return value is sbyte or byte or short or ushort or int or uint or long or ulong;
    }

    private static bool IsNumber(object value)
    {
        return IsInteger(value) || value is float or double or decimal;
    }

    private static void CheckNumericType(object input, ValueType valueType)
    {
        if (!(IsNumber(input) || (input is string s && DataTypeValidators.GetValidatorForType(valueType)(s))))
            throw new Exception($"Cannot convert value '{input}' to the XPath type {valueType} since it is not valid.");
    }


    private static object TransformXPathItemToObject(AbstractValue first,
        ExecutionParameters<TNode> executionParameters)
    {
        throw new NotImplementedException();
    }
    
    public static object? AdaptXPathValueToValue(
        ISequence valueSequence,
        SequenceType sequenceType,
        ExecutionParameters<TNode> executionParameters)
    {
        return sequenceType.Multiplicity switch
        {
            SequenceMultiplicity.ZeroOrOne when valueSequence.IsEmpty() => null,
            SequenceMultiplicity.ZeroOrOne => TransformValues<TNode>.TransformXPathItemToObject(valueSequence.First()!,
                    executionParameters)
                (IterationHint.None)
                .Value,
            SequenceMultiplicity.ZeroOrMore or SequenceMultiplicity.OneOrMore => valueSequence.GetAllValues()
                .Select(value =>
                {
                    if (value.GetValueType().IsSubtypeOf(ValueType.Attribute))
                        throw new Exception("Cannot pass attribute nodes to custom functions");

                    return TransformValues<TNode>
                        .TransformXPathItemToObject(value, executionParameters)(IterationHint.None).Value;
                })
                .ToArray(),
            _ => TransformValues<TNode>
                .TransformXPathItemToObject(valueSequence.First()!, executionParameters)(IterationHint.None).Value
        };
    }
}