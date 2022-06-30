using FontoXPathCSharp.Expressions.DataTypes.Facets;

namespace FontoXPathCSharp.Expressions.DataTypes.Builtins;

public class BuiltinDataTypes
{
    private BuiltinDataTypes()
    {
        BuiltInTypeModels.Instance.BuiltinModels.ToList().ForEach(model =>
        {
            var name = model.Name;
            var restrictionsByName = model.Restrictions;

            switch (model)
            {
                case { Variety: Variety.Primitive }:
                {
                    var parent = model.ParentType != null ? BuiltinDataTypesByType[model.ParentType] : null;
                    var validator = DataTypeValidators.GetValidatorForType(name);
                    var facetHandlers = TypeFacets.GetFacetsByDataType(name);
                    BuiltinDataTypesByType[name] = new TypeModel(Variety.Primitive, name, restrictionsByName, parent,
                        validator, facetHandlers, Array.Empty<TypeModel>());
                    return;
                }

                case { Variety: Variety.Derived }:
                {
                    var baseModel = BuiltinDataTypesByType[model.BaseType!];
                    var validator = DataTypeValidators.GetValidatorForType(name);
                    BuiltinDataTypesByType[name] = new TypeModel(Variety.Derived, name, restrictionsByName, baseModel,
                        validator, baseModel.TypeFacetHandlers, Array.Empty<TypeModel>());
                    return;
                }

                case { Variety: Variety.List }:
                {
                    var type = BuiltinDataTypesByType[model.Type!];
                    BuiltinDataTypesByType[name] = new TypeModel(Variety.List, name, restrictionsByName, type, null,
                        null, Array.Empty<TypeModel>());
                    return;
                }

                case { Variety: Variety.Union }:
                {
                    var memberTypes = model.MemberTypes!.Select(
                        memberTypeRef => BuiltinDataTypesByType[memberTypeRef]
                    ).ToArray();

                    BuiltinDataTypesByType[name] = new TypeModel(Variety.Union, name, restrictionsByName, null, null,
                        null, memberTypes);
                    return;
                }
            }
        });
    }

    public static BuiltinDataTypes Instance { get; } = new();

    public Dictionary<ValueType, TypeModel> BuiltinDataTypesByType { get; } = new();
}