using System.Text.RegularExpressions;
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
            WhitespaceHandling.Replace => Regex.Replace(input, "[\u0009\u000A\u000D]", " "),
            WhitespaceHandling.Collapse =>
                Regex.Replace(
                    Regex.Replace(
                        Regex.Replace(input, "[\u0009\u000A\u000D]", " "),
                    " {2,}", " "),
                "^ | $", ""),
            _ => throw new ArgumentOutOfRangeException()
        };
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