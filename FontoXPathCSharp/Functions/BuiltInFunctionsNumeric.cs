using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public class BuiltInFunctionsNumeric
{
    public static readonly FunctionSignature<ISequence> FnNumber = (_, executionParameters, _, sequences) =>
    {
        var sequence = sequences[0];
        var atomized = Atomize.AtomizeSequence(sequence, executionParameters);
        if (atomized.IsEmpty()) SequenceFactory.CreateFromValue(AtomicValue.Create(double.NaN, ValueType.XsDouble));
        if (atomized.IsSingleton())
        {
            return sequence.First()?.TryCastToType(ValueType.XsDouble) switch
            {
                SuccessResult<AtomicValue> result => SequenceFactory.CreateFromValue(result.Data),
                ErrorResult<AtomicValue> => SequenceFactory.CreateFromValue(AtomicValue.Create(double.NaN,
                    ValueType.XsDouble)),
                _ => throw new ArgumentOutOfRangeException(
                    $"BuiltInFunctionsNumeric: Unexpected parameter in fn:number: ({atomized.IsSingleton()}).")
            };
        }

        throw new XPathException("fn:number may only be called with zero or one values");
    };
    
    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[] { new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne) },
            FnNumber, "number",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsDouble, SequenceMultiplicity.ExactlyOne)),
        new(Array.Empty<ParameterType>(),
            (dynamicContext, executionParameters, staticContext, _) =>
            {
                var atomizedContextItem = dynamicContext?.ContextItem == null
                    ? null
                    : ArgumentHelper.PerformFunctionConversion(
                        new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne),
                        SequenceFactory.CreateFromValue(dynamicContext.ContextItem), executionParameters, "fn:number",
                        false);
                if (atomizedContextItem == null)
                {
                    throw new XPathException("XPDY0002: fn:number needs an atomizable context item.");
                }

                return FnNumber(dynamicContext, executionParameters, staticContext, atomizedContextItem);
            },
            "number",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsDouble, SequenceMultiplicity.ExactlyOne))
    };
}