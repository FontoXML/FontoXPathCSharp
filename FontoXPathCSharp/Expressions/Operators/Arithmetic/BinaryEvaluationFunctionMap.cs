using FontoXPathCSharp.Value.InternalValues;
using DateTime = FontoXPathCSharp.Value.InternalValues.DateTime;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators.Arithmetic;

public class BinaryEvaluationFunctionMap
{
    private static readonly Dictionary<(ValueType, ValueType, AstNodeName), ValueType> ReturnTypeMap = new()
    {
        {
            (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.IDivOp),
            ValueType.XsInteger
        },
        {
            (ValueType.XsYearMonthDuration, ValueType.XsYearMonthDuration, AstNodeName.AddOp),
            ValueType.XsYearMonthDuration
        },
        {
            (ValueType.XsYearMonthDuration, ValueType.XsYearMonthDuration, AstNodeName.SubtractOp),
            ValueType.XsYearMonthDuration
        },
        {
            (ValueType.XsYearMonthDuration, ValueType.XsYearMonthDuration, AstNodeName.DivOp),
            ValueType.XsDecimal
        },
        {
            (ValueType.XsYearMonthDuration, ValueType.XsNumeric, AstNodeName.MultiplyOp),
            ValueType.XsYearMonthDuration
        },
        {
            (ValueType.XsYearMonthDuration, ValueType.XsNumeric, AstNodeName.DivOp),
            ValueType.XsYearMonthDuration
        },
        {
            (ValueType.XsNumeric, ValueType.XsYearMonthDuration, AstNodeName.MultiplyOp),
            ValueType.XsYearMonthDuration
        },
        {
            (ValueType.XsDayTimeDuration, ValueType.XsDayTimeDuration, AstNodeName.AddOp),
            ValueType.XsDayTimeDuration
        },
        {
            (ValueType.XsDayTimeDuration, ValueType.XsDayTimeDuration, AstNodeName.SubtractOp),
            ValueType.XsDayTimeDuration
        },
        {
            (ValueType.XsDayTimeDuration, ValueType.XsDayTimeDuration, AstNodeName.DivOp),
            ValueType.XsDecimal
        },
        {
            (ValueType.XsDayTimeDuration, ValueType.XsNumeric, AstNodeName.MultiplyOp),
            ValueType.XsDayTimeDuration
        },
        {
            (ValueType.XsDayTimeDuration, ValueType.XsNumeric, AstNodeName.DivOp),
            ValueType.XsDayTimeDuration
        },
        {
            (ValueType.XsNumeric, ValueType.XsDayTimeDuration, AstNodeName.MultiplyOp),
            ValueType.XsDayTimeDuration
        },
        {
            (ValueType.XsDateTime, ValueType.XsDateTime, AstNodeName.SubtractOp),
            ValueType.XsDayTimeDuration
        },
        {
            (ValueType.XsDate, ValueType.XsDate, AstNodeName.SubtractOp),
            ValueType.XsDayTimeDuration
        },
        {
            (ValueType.XsTime, ValueType.XsTime, AstNodeName.SubtractOp),
            ValueType.XsDayTimeDuration
        },
        {
            (ValueType.XsDateTime, ValueType.XsYearMonthDuration, AstNodeName.AddOp),
            ValueType.XsDateTime
        },
        {
            (ValueType.XsDateTime, ValueType.XsYearMonthDuration, AstNodeName.SubtractOp),
            ValueType.XsDateTime
        },
        {
            (ValueType.XsDateTime, ValueType.XsDayTimeDuration, AstNodeName.AddOp),
            ValueType.XsDateTime
        },
        {
            (ValueType.XsDateTime, ValueType.XsDayTimeDuration, AstNodeName.SubtractOp),
            ValueType.XsDateTime
        },
        {
            (ValueType.XsDate, ValueType.XsDayTimeDuration, AstNodeName.AddOp),
            ValueType.XsDate
        },
        {
            (ValueType.XsDate, ValueType.XsDayTimeDuration, AstNodeName.SubtractOp),
            ValueType.XsDate
        },
        {
            (ValueType.XsDate, ValueType.XsYearMonthDuration, AstNodeName.AddOp),
            ValueType.XsDate
        },
        {
            (ValueType.XsDate, ValueType.XsYearMonthDuration, AstNodeName.SubtractOp),
            ValueType.XsDate
        },
        {
            (ValueType.XsTime, ValueType.XsDayTimeDuration, AstNodeName.AddOp),
            ValueType.XsTime
        },
        {
            (ValueType.XsTime, ValueType.XsDayTimeDuration, AstNodeName.SubtractOp),
            ValueType.XsTime
        }
    };


    private static readonly Dictionary<(ValueType, ValueType, AstNodeName), Func<object, object, object>> OperationMap =
        new()
        {
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.AddOp),
                (a, b) => Convert.ToDecimal(a) + Convert.ToDecimal(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.SubtractOp),
                (a, b) => Convert.ToDecimal(a) - Convert.ToDecimal(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.MultiplyOp),
                (a, b) => Convert.ToDecimal(a) * Convert.ToDecimal(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.DivOp),
                (a, b) => Convert.ToDecimal(a) / Convert.ToDecimal(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.ModOp),
                (a, b) => Convert.ToDecimal(a) % Convert.ToDecimal(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsNumeric, AstNodeName.IDivOp),
                (a, b) => Math.Truncate(Convert.ToDecimal(a) / Convert.ToDecimal(b))
            },
            {
                (ValueType.XsYearMonthDuration, ValueType.XsYearMonthDuration, AstNodeName.AddOp),
                (a, b) => (Duration)a + (Duration)b
            },
            {
                (ValueType.XsYearMonthDuration, ValueType.XsYearMonthDuration, AstNodeName.SubtractOp),
                (a, b) => (Duration)a - (Duration)b
            },
            {
                (ValueType.XsYearMonthDuration, ValueType.XsYearMonthDuration, AstNodeName.DivOp),
                (a, b) => (Duration)a / (Duration)b
            },
            {
                (ValueType.XsYearMonthDuration, ValueType.XsNumeric, AstNodeName.MultiplyOp),
                (a, b) => (Duration)a * Convert.ToDouble(b)
            },
            {
                (ValueType.XsYearMonthDuration, ValueType.XsNumeric, AstNodeName.DivOp),
                (a, b) => (Duration)a / Convert.ToDouble(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsYearMonthDuration, AstNodeName.MultiplyOp),
                (a, b) => (Duration)b * Convert.ToDouble(a)
            },
            {
                (ValueType.XsDayTimeDuration, ValueType.XsDayTimeDuration, AstNodeName.AddOp),
                (a, b) => (Duration)a + (Duration)b
            },
            {
                (ValueType.XsDayTimeDuration, ValueType.XsDayTimeDuration, AstNodeName.SubtractOp),
                (a, b) => (Duration)a - (Duration)b
            },
            {
                (ValueType.XsDayTimeDuration, ValueType.XsDayTimeDuration, AstNodeName.DivOp),
                (a, b) => (Duration)a / (Duration)b
            },
            {
                (ValueType.XsDayTimeDuration, ValueType.XsNumeric, AstNodeName.MultiplyOp),
                (a, b) => (Duration)a * Convert.ToDouble(b)
            },
            {
                (ValueType.XsDayTimeDuration, ValueType.XsNumeric, AstNodeName.DivOp),
                (a, b) => (Duration)a / Convert.ToDouble(b)
            },
            {
                (ValueType.XsNumeric, ValueType.XsDayTimeDuration, AstNodeName.MultiplyOp),
                (a, b) => (Duration)b * Convert.ToDouble(a)
            },
            {
                (ValueType.XsDateTime, ValueType.XsDateTime, AstNodeName.SubtractOp),
                (a, b) => (DateTime)a - (DateTime)b
            },
            {
                (ValueType.XsDate, ValueType.XsDate, AstNodeName.SubtractOp),
                (a, b) => (DateTime)a - (DateTime)b
            },
            {
                (ValueType.XsTime, ValueType.XsTime, AstNodeName.SubtractOp),
                (a, b) => (DateTime)a - (DateTime)b
            },
            {
                (ValueType.XsDateTime, ValueType.XsYearMonthDuration, AstNodeName.AddOp),
                (a, b) => (DateTime)a + (Duration)b
            },
            {
                (ValueType.XsDateTime, ValueType.XsYearMonthDuration, AstNodeName.SubtractOp),
                (a, b) => (DateTime)a - (Duration)b
            },
            {
                (ValueType.XsDateTime, ValueType.XsDayTimeDuration, AstNodeName.AddOp),
                (a, b) => (DateTime)a + (Duration)b
            },
            {
                (ValueType.XsDateTime, ValueType.XsDayTimeDuration, AstNodeName.SubtractOp),
                (a, b) => (DateTime)a - (Duration)b
            },
            {
                (ValueType.XsDate, ValueType.XsDayTimeDuration, AstNodeName.AddOp),
                (a, b) => (DateTime)a + (Duration)b
            },
            {
                (ValueType.XsDate, ValueType.XsDayTimeDuration, AstNodeName.SubtractOp),
                (a, b) => (DateTime)a - (Duration)b
            },
            {
                (ValueType.XsDate, ValueType.XsYearMonthDuration, AstNodeName.AddOp),
                (a, b) => (DateTime)a + (Duration)b
            },
            {
                (ValueType.XsDate, ValueType.XsYearMonthDuration, AstNodeName.SubtractOp),
                (a, b) => (DateTime)a - (Duration)b
            },
            {
                (ValueType.XsTime, ValueType.XsDayTimeDuration, AstNodeName.AddOp),
                (a, b) => (DateTime)a + (Duration)b
            },
            {
                (ValueType.XsTime, ValueType.XsDayTimeDuration, AstNodeName.SubtractOp),
                (a, b) => (DateTime)a - (Duration)b
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