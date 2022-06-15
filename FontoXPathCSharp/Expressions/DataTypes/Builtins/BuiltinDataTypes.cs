using FontoXPathCSharp.Expressions.DataTypes.Facets;

namespace FontoXPathCSharp.Expressions.DataTypes.Builtins;

public class BuiltinDataTypes
{
    private static BuiltinDataTypes? _instance;

    private readonly Dictionary<ValueType, TypeModel> _builtinDataTypesByType = new();

    private BuiltinDataTypes()
    {
        BuiltInTypeModels.GetInstance().BuiltinModels.ToList().ForEach(model =>
        {
            var name = model.Name;
            var restrictionsByName = model.Restrictions;

            switch (model)
            {
                case {Variety: Variety.Primitive}:
                {
                    var parent = model.ParentType != null ? _builtinDataTypesByType[model.ParentType] : null;
                    var validator = DataTypeValidators.GetValidatorForType(name);
                    var facetHandlers = TypeFacets.GetFacetsByDataType(name);
                    _builtinDataTypesByType[name] = new TypeModel(Variety.Primitive, name, restrictionsByName, parent,
                        validator, facetHandlers, Array.Empty<TypeModel>());
                    return;
                }

                case {Variety: Variety.Derived}:
                {
                    var baseModel = _builtinDataTypesByType[model.BaseType!];
                    var validator = DataTypeValidators.GetValidatorForType(name);
                    _builtinDataTypesByType[name] = new TypeModel(Variety.Derived, name, restrictionsByName, baseModel,
                        validator, baseModel.TypeFacetHandlers, Array.Empty<TypeModel>());
                    return;
                }

                case {Variety: Variety.List}:
                {
                    var type = _builtinDataTypesByType[model.Type!];
                    _builtinDataTypesByType[name] = new TypeModel(Variety.List, name, restrictionsByName, type, null,
                        null, Array.Empty<TypeModel>());
                    return;
                }

                case {Variety: Variety.Union}:
                {
                    var memberTypes = model.MemberTypes!.Select(
                        memberTypeRef => _builtinDataTypesByType[memberTypeRef]
                    ).ToArray();

                    _builtinDataTypesByType[name] = new TypeModel(Variety.Union, name, restrictionsByName, null, null,
                        null, memberTypes);
                    return;
                }
            }
        });
    }

    public static BuiltinDataTypes? GetInstance()
    {
        return _instance ??= new BuiltinDataTypes();
    }
}