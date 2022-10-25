using FontoXPathCSharp.Expressions.DataTypes.Casting;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public delegate Result<AtomicValue> CastingFunction(AtomicValue input);

public delegate bool InstanceOfFunction(params ValueType[] types);

public class TypeCasting
{
    private readonly Dictionary<int, CastingFunction> _precomputedCastFunctions = new();

    private readonly HashSet<ValueType> _treatAsPrimitive = new()
    {
        ValueType.XsNumeric,
        ValueType.XsInteger,
        ValueType.XsDayTimeDuration,
        ValueType.XsYearMonthDuration
    };

    private TypeCasting()
    {
    }

    public static TypeCasting Instance { get; } = new();


    private static Exception ProcessError(Error err)
    {
        return string.IsNullOrEmpty(err.Code)
            ? new Exception(err.Details)
            : new XPathException(err.Code, err.Details);
    }

    public static AtomicValue CastToType(AtomicValue value, ValueType type)
    {
        var result = TryCastToType(value, type);
        return result switch
        {
            ErrorResult<AtomicValue> errorResult => throw ProcessError(errorResult.Errors.First()),
            SuccessResult<AtomicValue> successResult => successResult.Data,
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
    }

    public static Result<AtomicValue> TryCastToType(AtomicValue value, ValueType type)
    {
        return Instance.TryCastToTypeInternal(value, type);
    }

    private Result<AtomicValue> TryCastToTypeInternal(AtomicValue value, ValueType type)
    {
        var index = (int)value.GetValueType() + (int)type * 10000;

        if (!_precomputedCastFunctions.ContainsKey(index))
            _precomputedCastFunctions[index] = CreateCastingFunction(value.GetValueType(), type);

        var prefabConverter = _precomputedCastFunctions[index];

        return prefabConverter(value);
    }

    private CastingFunction CreateCastingFunction(ValueType from, ValueType to)
    {
        if (from == ValueType.XsUntypedAtomic && to == ValueType.XsString)
            return value => new SuccessResult<AtomicValue>(AtomicValue.Create(value.GetValue(), ValueType.XsString));

        if (to == ValueType.XsNotation)
            return _ => new ErrorResult<AtomicValue>("Casting to xs:NOTATION is not permitted.",
                new Error[] { new("XPST0080", "Casting to xs:NOTATION is not permitted.") });

        if (to == ValueType.XsError)
            return _ => new ErrorResult<AtomicValue>("Casting to xs:error is not permitted.",
                new Error[] { new("FORG0001", "Casting to xs:error is not permitted.") });

        if (from == ValueType.XsAnySimpleType || to == ValueType.XsAnySimpleType)
            return _ => new ErrorResult<AtomicValue>("Casting from or to xs:anySimpleType is not permitted.",
                new Error[] { new("XPST0080", "Casting from or to xs:anySimpleType is not permitted.") });

        if (from == ValueType.XsAnyAtomicType || to == ValueType.XsAnyAtomicType)
            return _ => new ErrorResult<AtomicValue>(
                "Casting from or to xs: anyAtomicType is not permitted.",
                new Error[] { new("XPST0080", "Casting from or to xs: anyAtomicType is not permitted.") });

        if (from.IsSubtypeOf(ValueType.Function) && to == ValueType.XsString)
            return _ =>
                new ErrorResult<AtomicValue>("Casting from function item to xs:string is not permitted.",
                    new Error[] { new("FOTY0014", "Casting from function item to xs:string is not permitted.") });

        if (from == to) return value => new SuccessResult<AtomicValue>(value);

        var primitiveFromNullable = _treatAsPrimitive.Contains(from) ? from : TypeHelpers.GetPrimitiveTypeName(from);
        var primitiveToNullable = _treatAsPrimitive.Contains(to) ? to : TypeHelpers.GetPrimitiveTypeName(to);

        if (primitiveFromNullable == null || primitiveToNullable == null)
            return _ => new ErrorResult<AtomicValue>(
                $"Can not cast: type {(primitiveToNullable != null ? from : to)} is unknown.",
                new Error[]
                {
                    new("XPST0081", $"Can not cast: type {(primitiveToNullable != null ? from : to)} is unknown.")
                });

        // Compiler was being difficult, this was the only way to make it stop.
        var primitiveFrom = (ValueType)primitiveFromNullable;
        var primitiveTo = (ValueType)primitiveToNullable;

        var converters = new List<CastingFunction>();

        if (primitiveFrom.IsSubtypeOfAny(ValueType.XsString, ValueType.XsUntypedAtomic))
            converters.Add(value =>
            {
                // Not sure if this is correct, it seems more correct than the original code though.
                var strValue =
                    TypeHelpers.NormalizeWhitespace(value.CastToType(ValueType.XsString).GetAs<StringValue>().Value,
                        to);
                if (!TypeHelpers.ValidatePattern(strValue, to))
                    return new ErrorResult<AtomicValue>(
                        $"Cannot cast {value} to {to}, pattern validation failed.",
                        new Error[] { new("FORG0001", $"Cannot cast {value} to {to}, pattern validation failed.") });

                return new SuccessResult<AtomicValue>(new StringValue(strValue));
            });

        if (primitiveFrom != primitiveTo)
        {
            // Same for this one.
            converters.Add(CastToPrimitiveType(primitiveFrom, primitiveTo));
            converters.Add(val => new SuccessResult<AtomicValue>(AtomicValue.Create(val.GetValue(), primitiveFrom)));
        }

        if (primitiveTo.IsSubtypeOfAny(ValueType.XsString, ValueType.XsUntypedAtomic))
            converters.Add(value =>
            {
                if (!TypeHelpers.ValidatePattern(
                        Convert.ToString(value.GetAs<AtomicValue>().GetValue()) ?? string.Empty, to))
                    return new ErrorResult<AtomicValue>(
                        $"Cannot cast {value} to {to}, pattern validation failed.",
                        new Error[] { new("FORG0001", $"Cannot cast {value} to {to}, pattern validation failed.") });

                return new SuccessResult<AtomicValue>(AtomicValue.Create(value.GetValue(), primitiveTo));
            });

        converters.Add(value => new SuccessResult<AtomicValue>(AtomicValue.Create(value.GetValue(), to)));

        return value =>
        {
            Result<AtomicValue> result = new SuccessResult<AtomicValue>(value);
            foreach (var converter in converters)
            {
                result = converter(result.Data);
                if (result.Failure) return result;
            }

            return result;
        };
    }

    private static CastingFunction CastToPrimitiveType(ValueType from, ValueType to)
    {
        if (to == ValueType.XsError)
            return _ => new ErrorResult<AtomicValue>("Casting to xs:error is always invalid.",
                new Error[] { new("FORG0001", "Casting to xs:error is always invalid.") });

        return to switch
        {
            ValueType.XsUntypedAtomic => CastToUntypedAtomic.ToUntypedAtomic(from),
            ValueType.XsString => CastToString.ToString(from),
            ValueType.XsFloat => CastToFloat.ToFloat(from),
            ValueType.XsDouble => CastToDouble.ToDouble(from),
            ValueType.XsDecimal => CastToDecimal.ToDecimal(from),
            ValueType.XsInteger => CastToInteger.ToInteger(from),
            ValueType.XsBoolean => CastToBoolean.ToBoolean(from),
            _ => _ => throw new NotImplementedException($"Type casting to {to} has not been implemented yet.")
        };
    }
}