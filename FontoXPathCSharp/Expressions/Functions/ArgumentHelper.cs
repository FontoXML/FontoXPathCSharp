using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Functions;

public class ArgumentHelper<TNode> where TNode : notnull
{
    public static AtomicValue? PromoteToType(AbstractValue value, ValueType type)
    {
        if (value.GetValueType().IsSubtypeOf(ValueType.XsNumeric))
        {
            if (value.GetValueType().IsSubtypeOf(ValueType.XsFloat))
                return type == ValueType.XsDouble
                    ? AtomicValue.Create(value.GetAs<FloatValue>().Value, type)
                    : null;

            if (value.GetValueType().IsSubtypeOf(ValueType.XsDecimal))
            {
                if (value.GetValueType().IsSubtypeOf(ValueType.XsInteger))
                    switch (type)
                    {
                        case ValueType.XsFloat:
                            return AtomicValue.Create(value.GetAs<IntegerValue>().Value, type);
                        case ValueType.XsDouble:
                            return AtomicValue.Create(value.GetAs<IntegerValue>().Value, type);
                    }

                switch (type)
                {
                    case ValueType.XsFloat:
                        return AtomicValue.Create(value.GetAs<DecimalValue>().Value, type);
                    case ValueType.XsDouble:
                        return AtomicValue.Create(value.GetAs<DecimalValue>().Value, type);
                }
            }

            return null;
        }

        if (value.GetValueType().IsSubtypeOf(ValueType.XsAnyUri))
            if (type == ValueType.XsString)
                throw new NotImplementedException("xs:anyUri is not implemented yet, so this cannot be done yet.");
        // return AtomicValue.Create(value.value, type);
        return null;
    }

    private static AbstractValue MapItem(
        AbstractValue argumentItem,
        SequenceType seqType,
        ExecutionParameters<TNode> executionParameters,
        string functionName,
        bool isReturn)
    {
        if (argumentItem.GetValueType().IsSubtypeOf(seqType.ValueType)) return argumentItem;

        if (
                seqType.ValueType.IsSubtypeOf(ValueType.XsAnyAtomicType) &&
                argumentItem.GetValueType().IsSubtypeOf(ValueType.Node)
            )
            // Assume here that a node always atomizes to a singlevalue. This will not work
            // anymore when schema support will be imlemented.
            argumentItem = Atomize.AtomizeSingleValue(argumentItem, executionParameters).First()!;

        // Maybe after atomization, we have the correct type
        if (argumentItem.GetValueType().IsSubtypeOf(seqType.ValueType)) return argumentItem;

        // Everything is an anyAtomicType, so no casting necessary.
        if (seqType.ValueType == ValueType.XsAnyAtomicType) return argumentItem;
        if (argumentItem.GetValueType().IsSubtypeOf(ValueType.XsUntypedAtomic))
        {
            // We might be able to cast this to the wished type
            var convertedItem = argumentItem.CastToType(seqType.ValueType);
            if (convertedItem == null)
                throw new XPathException(
                    "XPTY0004",
                    $"Unable to convert {(isReturn ? "return" : "argument")} of type " +
                    $"{argumentItem.GetValueType().Name()} to type {seqType} while calling '{functionName}'"
                );
            return convertedItem;
        }

        // We need to promote this
        var item = PromoteToType(argumentItem, seqType.ValueType);
        if (item == null)
            throw new XPathException(
                "XPTY0004",
                $"Unable to cast {(isReturn ? "return" : "argument")} of type " +
                $"{argumentItem.GetValueType().Name()} to type {seqType} while calling '{functionName}'");
        return item;
    }

    public static ISequence PerformFunctionConversion(
        SequenceType argumentType,
        ISequence argument,
        ExecutionParameters<TNode> executionParameters,
        string functionName,
        bool isReturn)
    {
        if (argumentType.Multiplicity == SequenceMultiplicity.ZeroOrOne)
            return argument.IsEmpty() || argument.IsSingleton()
                ? argument.Map((value, _, _) =>
                    MapItem(value, argumentType, executionParameters, functionName, isReturn))
                : throw new XPathException(
                    "XPTY0004",
                    $"Multiplicity of {(isReturn ? "function return value" : "function argument")} " +
                    $"of type {argumentType} for '{functionName}' is incorrect. Expected '?' but got '+'.");

        if (argumentType.Multiplicity == SequenceMultiplicity.OneOrMore)
            return argument.IsEmpty()
                ? throw new XPathException(
                    "XPTY0004",
                    $"Multiplicity of {(isReturn ? "function return value" : "function argument")} " +
                    $"of type {argumentType} for '{functionName}' is incorrect. Expected '+' but got 'empty-sequence()'")
                : argument.Map((value, _, _) =>
                    MapItem(value, argumentType, executionParameters, functionName, isReturn));

        if (argumentType.Multiplicity == SequenceMultiplicity.ZeroOrMore)
            return argument.Map((value, _, _) =>
                MapItem(value, argumentType, executionParameters, functionName, isReturn));

        return argument.IsSingleton()
            ? argument.Map((value, _, _) =>
                MapItem(value, argumentType, executionParameters, functionName, isReturn))
            : throw new XPathException(
                "XPTY0004",
                $"Multiplicity of {(isReturn ? "function return value" : "function argument")} of type {argumentType} for '{functionName}' is incorrect. Expected exactly one");
    }
}