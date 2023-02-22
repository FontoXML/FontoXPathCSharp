using System.Reflection;

namespace FontoXPathCSharp.EvaluationUtils;

public class ReflectionCast
{
    private static readonly Dictionary<Type, (MethodInfo, MethodInfo)> ArrayCastFunctions = new();

    // Reflection equivalent of calling list.Cast<U>().ToArray(), when T = U[]
    public static T CastToArrayDynamically<T>(Array arr)
    {
        var elementType = typeof(T).GetElementType()!;
        if (!ArrayCastFunctions.ContainsKey(elementType))
            ArrayCastFunctions[elementType] = (
                typeof(Enumerable)
                    .GetMethod("Cast", BindingFlags.Static | BindingFlags.Public)!
                    .MakeGenericMethod(elementType),
                typeof(Enumerable)
                    .GetMethod("ToArray", BindingFlags.Static | BindingFlags.Public)!
                    .MakeGenericMethod(elementType)
            );

        var (cast, toArray) = ArrayCastFunctions[elementType];
        var casted = cast.Invoke(null, new[] { arr });
        var array = toArray.Invoke(null, new[] { casted });
        return (T)array!;
    }
}