using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToString
{
    public static CastingFunction ToString(InstanceOfFunction instanceOf)
    {
        var caster = ToStringLikeType(instanceOf);
        return value =>
        {
            var castResult = caster(value);
            return castResult switch
            {
                ErrorResult<AtomicValue> e => e,
                SuccessResult<AtomicValue> r => new SuccessResult<AtomicValue>(Atomize.CreateAtomicValue(r.Data, ValueType.XsString))
            };
        };
    }

    private static CastingFunction ToStringLikeType(InstanceOfFunction instanceOf)
    {
	    if (instanceOf(ValueType.XsString, ValueType.XsUntypedAtomic))
	    {
		    return value => new SuccessResult<AtomicValue>(Atomize.CreateAtomicValue(value + "", ValueType.XsString));
	    }
		if (instanceOf(ValueType.XsAnyUri)) {
			return value => new SuccessResult<AbstractValue>(value, )
			// return (value) => ({
			// 	successful: true,
			// 	value,
			// });
		}
		if (instanceOf(ValueType.XsQName)) {
			// return (value: QName) => {
			// 	return {
			// 		successful: true,
			// 		value: value.prefix ? `${value.prefix}:${value.localName}` : value.localName,
			// 	};
			// };
		}
		if (instanceOf(ValueType.XsNotation)) {
			// return (value) => ({
			// 	successful: true,
			// 	value: value.toString(),
			// });
		}
		if (instanceOf(ValueType.XsNumeric)) {
			if (instanceOf(ValueType.XsInteger, ValueType.XsDecimal)) {
				// return (value) => ({
				// 	successful: true,
				// 	value: (value + '').replace('e', 'E'),
				// });
			}
			if (instanceOf(ValueType.XsFloat) || instanceOf(ValueType.XsDouble)) {
				return (value) => {
					if (isNaN(value)) {
						// return {
						// 	successful: true,
						// 	value: 'NaN',
						// };
					}
					if (!isFinite(value)) {
						// return {
						// 	successful: true,
						// 	value: `${value < 0 ? '-' : ''}INF`,
						// };
					}
					// if (Object.is(value, -0)) {
					// 	return {
					// 		successful: true,
					// 		value: '-0',
					// 	};
					// }
					// Use Javascript's built in number formatting. This outputs like 1e+100. The valid XPath version is
					// 1E100: without the +, and with the exponent in capitals
					// return {
					// 	successful: true,
					// 	value: (value + '').replace('e', 'E').replace('E+', 'E'),
					// };
				};
			}
		}
		if (
			instanceOf(ValueType.XsDateTime,ValueType.XsDate,ValueType.XsTime,ValueType.XsGDay,ValueType.XsGMonth,ValueType.XsGMonthDay,ValueType.XsGYear,ValueType.XsGYearMonth)
		) {
			// return (value) => ({
			// 	successful: true,
			// 	value: value.toString(),
			// });
		}
		if (instanceOf(ValueType.XsYearMonthDuration)) {
			// return (value) => ({
			// 	successful: true,
			// 	value: value.toString(),
			// });
		}
		if (instanceOf(ValueType.XsDayTimeDuration)) {
			// return (value) => ({
			// 	successful: true,
			// 	value: value.toString(),
			// });
		}
		if (instanceOf(ValueType.XsDuration)) {
			// return (value) => ({
			// 	successful: true,
			// 	value: value.toString(),
			// });
		}
		if (instanceOf(ValueType.XsHexBinary)) {
			// return (value) => ({
			// 	successful: true,
			// 	value: value.toUpperCase(),
			// });
		}
		// return (value) => ({
		// 	successful: true,
		// 	value: value + '',
		// });
    }
}