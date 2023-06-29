using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToAnyUri
{
	public static CastingFunction ToAnyUri(ValueType from)
	{
		if (from.IsSubtypeOfAny(ValueType.XsString, ValueType.XsUntypedAtomic))
		{
				return value =>
					new SuccessResult<AtomicValue>(AtomicValue.Create(value, ValueType.XsAnyUri));
		}

		return _ => new ErrorResult<AtomicValue>(
			$"Casting not supported from {from.Name()} to xs:anyUri or any of its derived types.", "XPTY0004");
	}
}
