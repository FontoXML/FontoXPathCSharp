using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class SubtypeUtils
{
    public static bool IsSubtypeOfType(TypeModel subType, TypeModel superType)
    {
        if (superType.Variety == Variety.Union)
            return Array.Find(
                superType.MemberTypes,
                memberType => IsSubtypeOfType(subType, memberType)) != null;

        var tempSubtype = subType;

        while (tempSubtype != null)
        {
            if (tempSubtype.Type == superType.Type) return true;
            if (tempSubtype.Variety == Variety.Union)
                return Array.Find(
                    tempSubtype.MemberTypes,
                    memberType => IsSubtypeOf(memberType.Type, superType.Type)) != null;

            tempSubtype = tempSubtype.Parent;
        }

        return false;
    }

    public static bool IsSubtypeOf(ValueType baseSubType, ValueType baseSuperType)
    {
        if (baseSubType == baseSuperType) return true;

        

        // var superType = builtinDataTypesByType[baseSuperType];
        // var subType = builtinDataTypesByType[baseSubType];
        //
        // return IsSubtypeOfType(subType, superType);
    }

    /**
     * Utility function, since the pattern where a subtype is checked against a whole list of supertypes is very common.
     */
    public static bool IsSubTypeOfAny(ValueType baseSubType, IEnumerable<ValueType> baseSuperType)
    {
        return baseSuperType.Any(superType => IsSubtypeOf(baseSubType, superType));
    }
}