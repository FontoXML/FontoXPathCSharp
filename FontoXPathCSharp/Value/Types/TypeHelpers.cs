using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Expressions.DataTypes.Builtins;

namespace FontoXPathCSharp.Value.Types;

public class TypeHelpers
{
    public static ValueType? GetPrimitiveTypeName(ValueType typeName)
    {
        var type = BuiltinDataTypes.Instance.BuiltinDataTypesByType[typeName];
        while (type != null && type.Variety != Variety.Primitive) type = type.Parent;
        return type?.Type;
    }

    public static string NormalizeWhitespace(string input, ValueType typeName)
    {
        var type = BuiltinDataTypes.Instance.BuiltinDataTypesByType[typeName];
        var restrictionsByName = type.RestrictionsByName;
        if (restrictionsByName?.Whitespace == null)
            return type.Parent == null ? input : NormalizeWhitespace(input, type.Parent.Type);
        var whiteSpaceType = type.RestrictionsByName!.Whitespace;
        return whiteSpaceType switch
        {
            WhitespaceHandling.Preserve => input,
            WhitespaceHandling.Replace => throw new NotImplementedException(),
            WhitespaceHandling.Collapse => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };

        // Replace:
        // return input.replace(/[\u0009\u000A\u000D]/g, ' ');

        // Collapse:
        // return input
        //     .replace(/[\u0009\u000A\u000D]/g, ' ')
        //     .replace(/ {2,}/g, ' ')
        //     .replace(/^ | $/g, '');
    }

    public static bool ValidatePattern(string input, ValueType type)
    {
        var typeModel = BuiltinDataTypes.Instance.BuiltinDataTypesByType[type];
        while (typeModel != null && typeModel.Validator == null)
        {
            if (typeModel.Variety is Variety.List or Variety.Union) return true;
            typeModel = typeModel.Parent;
        }

        return typeModel == null || typeModel.Validator!(input);
    }
}