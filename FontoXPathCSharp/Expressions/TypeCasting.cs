using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

using CastingFunction = Func<AbstractValue, Result<AtomicValue>>;

public class TypeCasting
{
    private readonly HashSet<ValueType> _treatAsPrimitive = new()
    {
        ValueType.XsNumeric,
        ValueType.XsInteger,
        ValueType.XsDayTimeDuration,
        ValueType.XsYearMonthDuration,
    };

    private readonly Dictionary<int, CastingFunction> _precomputedCastFunctions = new();
    public static TypeCasting Instance { get; } = new();

    public Result<AbstractValue> TryCastToType()
    {
        throw new NotImplementedException();
    }

    private TypeCasting()
    {
    }

    public static AtomicValue CastToType(AtomicValue value, ValueType type)
    {
        var result = Instance.TryCastToType(value, type);
        return result switch
        {
            ErrorResult<AtomicValue> errorResult => throw new Exception(errorResult.Message),
            SuccessResult<AtomicValue> successResult => successResult.Data,
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
    }

    private Result<AtomicValue> TryCastToType(AtomicValue value, ValueType type)
    {
        var index = (int)value.GetValueType() + (int)type * 10000;

        if (!_precomputedCastFunctions.ContainsKey(index))
        {
            _precomputedCastFunctions[index] = CreateCastingFunction(value.GetValueType(), type);
        }

        var prefabConverter = _precomputedCastFunctions[index];

        return prefabConverter(value);
    }

    private CastingFunction CreateCastingFunction(ValueType from, ValueType to)
    {
        if (from == ValueType.XsUntypedAtomic && to == ValueType.XsString)
        {
            return value => new SuccessResult<AtomicValue>(Atomize.CreateAtomicValue(value, ValueType.XsString));
        }

        if (to == ValueType.XsNotation)
        {
            return _ => new ErrorResult<AtomicValue>("XPST0080: Casting to xs:NOTATION is not permitted.");
        }

        if (to == ValueType.XsError)
        {
            return _ => new ErrorResult<AtomicValue>("FORG0001: Casting to xs:error is not permitted.");
        }

        if (from == ValueType.XsAnySimpleType || to == ValueType.XsAnySimpleType)
        {
            return _ => new ErrorResult<AtomicValue>("XPST0080: Casting from or to xs:anySimpleType is not permitted.");
        }

        if (from == ValueType.XsAnyAtomicType || to == ValueType.XsAnyAtomicType)
        {
            return _ => new ErrorResult<AtomicValue>(
                "XPST0080: Casting from or to xs: anyAtomicType is not permitted.");
        }

        if (SubtypeUtils.IsSubtypeOf(from, ValueType.Function) && to == ValueType.XsString)
        {
            return _ =>
                new ErrorResult<AtomicValue>("FOTY0014: Casting from function item to xs:string is not permitted.");
        }

        if (from == to)
        {
            return value => new SuccessResult<AtomicValue>(Atomize.CreateAtomicValue(value, to));
        }

        var primitiveFromNullable = _treatAsPrimitive.Contains(from) ? from : TypeHelpers.GetPrimitiveTypeName(from);
        var primitiveToNullable = _treatAsPrimitive.Contains(to) ? to : TypeHelpers.GetPrimitiveTypeName(to);

        if (primitiveFromNullable == null || primitiveToNullable == null)
        {
            return _ => new ErrorResult<AtomicValue>(
                $"XPST0081: Can not cast: type {(primitiveToNullable != null ? from : to)} is unknown.");
        }

        // Compiler was being difficult, this was the only way to make it stop.
        var primitiveFrom = (ValueType)primitiveFromNullable;
        var primitiveTo = (ValueType)primitiveToNullable;

        var converters = new List<CastingFunction>();

        if (SubtypeUtils.IsSubTypeOfAny(primitiveFrom, new[] { ValueType.XsString, ValueType.XsUntypedAtomic }))
        {
            converters.Add(value =>
            {
                // Not sure if this is correct, it seems more correct than the original code though.
                var strValue = TypeHelpers.NormalizeWhitespace(value.GetAs<StringValue>(ValueType.XsString)!.Value, to);
                if (!TypeHelpers.ValidatePattern(strValue, to))
                {
                    return new ErrorResult<AtomicValue>(
                        $"FORG0001: Cannot cast ${value} to ${to}, pattern validation failed.");
                }

                return new SuccessResult<AtomicValue>(Atomize.CreateAtomicValue(strValue, to));
            });
        }

        if (primitiveFrom != primitiveTo)
        {
            // Same for this one.
            converters.Add(CastToPrimitiveType(primitiveFrom, primitiveTo));
            converters.Add(val => new SuccessResult<AtomicValue>(Atomize.CreateAtomicValue(val, val.GetValueType())));
        }

        if (SubtypeUtils.IsSubTypeOfAny(primitiveTo, new[] { ValueType.XsString, ValueType.XsUntypedAtomic }))
        {
            converters.Add(value =>
            {
                if (!TypeHelpers.ValidatePattern(value.GetAs<StringValue>(ValueType.XsString)!.Value, to))
                {
                    return new ErrorResult<AtomicValue>(
                        $"FORG0001: Cannot cast ${value} to ${to}, pattern validation failed.");
                }

                return new SuccessResult<AtomicValue>(Atomize.CreateAtomicValue(value, value.GetValueType()));
            });
        }

        converters.Add(value => new SuccessResult<AtomicValue>(Atomize.CreateAtomicValue(value, to)));

        return value =>
        {
            Result<AtomicValue> result =
                new SuccessResult<AtomicValue>(Atomize.CreateAtomicValue(value, value.GetValueType()));
            foreach (var converter in converters)
            {
                result = converter(result.Data);
                if (result.Failure) return result;
            }

            return result;
        };
    }

    public static CastingFunction CastToPrimitiveType(ValueType from, ValueType to)
    {
        var instanceOf = new Func<ValueType, bool>(type => SubtypeUtils.IsSubtypeOf(from, type));

        if (to == ValueType.XsError)
        {
            return _ => new ErrorResult<AtomicValue>("FORG0001: Casting to xs:error is always invalid.");
        }

        switch (to)
        {
            case ValueType.XsUntypedAtomic:
            case ValueType.XsString:
            case ValueType.XsFloat:
            case ValueType.XsDouble:
            case ValueType.XsDecimal:
            case ValueType.XsInteger:
            case ValueType.XsNumeric:
            case ValueType.XsDuration:
            case ValueType.XsYearMonthDuration:
            case ValueType.XsDayTimeDuration:
            case ValueType.XsDateTime:
            case ValueType.XsTime:
            case ValueType.XsDate:
            case ValueType.XsGYearMonth:
            case ValueType.XsGYear:
            case ValueType.XsGMonthDay:
            case ValueType.XsGDay:
            case ValueType.XsGMonth:
            case ValueType.XsBoolean:
            case ValueType.XsBase64Binary:
            case ValueType.XsHexBinary:
            case ValueType.XsAnyUri:
            case ValueType.XsQName:
                throw new NotImplementedException("Type casting between primitives not implemented yet.");
        }

        return _ => new ErrorResult<AtomicValue>($"XPTY0004: Casting not supported from ${from} to ${to}.");
    }
}