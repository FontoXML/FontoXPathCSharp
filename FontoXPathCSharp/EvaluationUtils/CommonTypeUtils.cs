using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.EvaluationUtils;

public class CommonTypeUtils
{
    public static AbstractValue?[]? ConvertItemsToCommonType(AbstractValue?[]? items)
    {
        if (items.All(item =>
            {
                // xs:integer is the only numeric type with inherits from another numeric type
                return item == null ||
                       item.GetValueType().IsSubtypeOf(ValueType.XsInteger) ||
                       item.GetValueType().IsSubtypeOf(ValueType.XsInteger);
            })
           )
            // They are all integers, we do not have to convert them to decimals
            return items;

        var commonTypeName = items
            .Select(item => item != null ? TypeHelpers.GetPrimitiveTypeName(item.GetValueType()) : null)
            .Aggregate((typeName, itemType) =>
            {
                if (itemType == null) return typeName;

                return itemType == typeName ? typeName : null;
            });

        if (commonTypeName != null)
            // All items are already of the same type
            return items;

        // If each value is an instance of one of the types xs:string or xs:anyURI, then all the values are cast to type xs:string
        if (
            items.All(item => item == null
                              || item.GetValueType().IsSubtypeOf(ValueType.XsString)
                              || item.GetValueType().IsSubtypeOf(ValueType.XsAnyUri))
        )
            return items.Select(item => (AbstractValue?)item?.CastToType(ValueType.XsString)).ToArray();

        // If each value is an instance of one of the types xs:decimal or xs:float, then all the values are cast to type xs:float.
        if (
            items.All(item => item == null
                              || item.GetValueType().IsSubtypeOf(ValueType.XsDecimal)
                              || item.GetValueType().IsSubtypeOf(ValueType.XsFloat))
        )
            return items.Select(item => (AbstractValue?)item?.CastToType(ValueType.XsFloat)).ToArray();

        // If each value is an instance of one of the types xs:decimal, xs:float, or xs:double, then all the values are cast to type xs:double.
        if (
            items.All(item => item == null
                              || item.GetValueType().IsSubtypeOf(ValueType.XsDecimal)
                              || item.GetValueType().IsSubtypeOf(ValueType.XsFloat)
                              || item.GetValueType().IsSubtypeOf(ValueType.XsDouble))
        )
            return items.Select(item => (AbstractValue?)item?.CastToType(ValueType.XsDouble)).ToArray();

        // Otherwise, a type error is raised. The exact error type is determined by the caller.
        return null;
    }
}