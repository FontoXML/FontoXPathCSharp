using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.EvaluationUtils;

public class ParameterUtils
{
    public static Dictionary<string, AbstractValue?> ConvertToAbstractValueVariables<TNode>(
        Dictionary<string, object> variablesMap, 
        IDomFacade<TNode> domFacade)
    {
        var returnVariables = new Dictionary<string, AbstractValue?>();
        foreach (var (varName, varVal) in variablesMap) returnVariables.Add(varName, ConvertToAbstractValue<TNode>(varVal, domFacade));

        return returnVariables;
    }

    private static AbstractValue? ConvertToAbstractValue<TNode>(object? value, IDomFacade<TNode> domFacade)
    {
        if (value == null) return null;
        var actualType = value.GetType();
        if (actualType == typeof(bool)) return AtomicValue.Create(value, ValueType.XsBoolean);
        if (actualType == typeof(string)) return AtomicValue.Create(value, ValueType.XsString);
        if (actualType == typeof(int)) return AtomicValue.Create(value, ValueType.XsInt);
        if (actualType == typeof(float)) return AtomicValue.Create(value, ValueType.XsFloat);
        if (actualType == typeof(double)) return AtomicValue.Create(value, ValueType.XsDouble);
        if (actualType == typeof(decimal)) return AtomicValue.Create(value, ValueType.XsDecimal);
        if (actualType == typeof(QName)) return AtomicValue.Create(value, ValueType.XsQName);
        if (actualType.IsAssignableTo(typeof(TNode))) return new NodeValue<TNode>((TNode)value, domFacade);
        throw new NotImplementedException(
            $"The type {actualType} is not supported yet for automatic conversion to AbstractValues. " +
            "Only a subset set of Atomic Values are implemented as variables so far.");
    }

    public static TNode VerifyContextNode<TNode>(TNode contextNode, IDomFacade<TNode> domFacade)
        where TNode : notnull
    {
        if (contextNode == null)
            throw new Exception("Cannot create a context node from a null value.");

        if (domFacade == null)
            throw new Exception("Cannot have a null DOM facade when context item is an XML node.");

        return contextNode;
    }
}