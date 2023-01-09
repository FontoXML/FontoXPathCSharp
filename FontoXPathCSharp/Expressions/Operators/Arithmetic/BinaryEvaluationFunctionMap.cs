using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators.Arithmetic;

public class BinaryEvaluationFunctionMap
{
    private static readonly Dictionary<(ValueType, ValueType, AstNodeName), ValueType> ReturnTypeMap = new()
    {
        { (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.IDivOp), ValueType.XsInteger }
    };


    private static readonly Dictionary<(ValueType, ValueType, AstNodeName), Func<object, object, object>> OperationMap =
        new()
        {
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.AddOp),
                (a, b) => Convert.ToDouble(a) + Convert.ToDouble(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.SubtractOp),
                (a, b) => Convert.ToDouble(a) - Convert.ToDouble(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.MultiplyOp),
                (a, b) => Convert.ToDouble(a) * Convert.ToDouble(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.DivOp),
                (a, b) => Convert.ToDouble(a) / Convert.ToDouble(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.ModOp),
                (a, b) => Convert.ToDouble(a) % Convert.ToDouble(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.IDivOp),
                (a, b) => Math.Truncate(Convert.ToDouble(a) / Convert.ToDouble(b))
            }
        };

    public static ValueType? GetReturnTypeForOperands(ValueType lhs, ValueType rhs, AstNodeName op)
    {
        var tuple = (lhs, rhs, op);
        return ReturnTypeMap.ContainsKey(tuple) ? ReturnTypeMap[tuple] : null;
    }

    public static Func<object, object, object>? GetOperationForOperands(ValueType lhs, ValueType rhs,
        AstNodeName op)
    {
        var tuple = (lhs, rhs, op);
        return OperationMap.ContainsKey(tuple) ? OperationMap[tuple] : null;
    }
}