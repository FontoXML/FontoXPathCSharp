using FontoXPathCSharp.Expressions.Functions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class FunctionCall<TNode> : PossiblyUpdatingExpression<TNode> where TNode : notnull
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly XPathException Xpty0004 = new(
        "XPTY0004",
        "Expected base expression of a function call to evaluate to a sequence of single function item"
    );

    private readonly AbstractExpression<TNode>?[] _argumentExpressions;
    private readonly int _callArity;
    private readonly AbstractExpression<TNode> _functionReferenceExpression;
    private readonly bool[] _isGapByOffset;
    private FunctionValue<ISequence, TNode>? _functionReference;
    private StaticContext<TNode>? _staticContext;

    public FunctionCall(
        AbstractExpression<TNode> functionReference,
        AbstractExpression<TNode>?[] args) : base(
        new Specificity(SpecificityKind.External, 1),
        new[] { functionReference }.Concat(args.Where(arg => arg != null)).ToArray(),
        new OptimizationOptions(false))
    {
        _callArity = args.Length;
        _isGapByOffset = args.Select(arg => arg == null).ToArray();
        _staticContext = null;
        _functionReferenceExpression = functionReference;
        _argumentExpressions = args;
    }

    public override ISequence PerformFunctionalEvaluation(
        DynamicContext? dynamicContext,
        ExecutionParameters<TNode> executionParameters,
        SequenceCallback[] createArgumentSequences)
    {
        // These two are deconstructed with
        // createFunctionReferenceSequence, ...createArgumentSequences in the TS version.
        var createFunctionReferenceSequence = createArgumentSequences[0];
        createArgumentSequences = createArgumentSequences.Skip(1).ToArray();

        if (_functionReference != null)
            return CallFunction(
                _functionReference,
                (innerDynamicContext, innerExecutionParameters, staticContext, args) =>
                    _functionReference.Value(innerDynamicContext, innerExecutionParameters, staticContext, args),
                dynamicContext,
                executionParameters,
                _isGapByOffset,
                createArgumentSequences,
                _staticContext);

        var sequence = createFunctionReferenceSequence(dynamicContext!);
        if (!sequence.IsSingleton()) throw Xpty0004;

        return sequence.MapAll(item =>
        {
            var functionItem = ValidateFunctionItem<ISequence>(item[0], _callArity);

            if (functionItem.IsUpdating)
                throw new XPathException(
                    "XUDY0038",
                    "The function returned by the PrimaryExpr of a dynamic function invocation can not be an updating function");

            return CallFunction(
                functionItem,
                functionItem.Value,
                dynamicContext!,
                executionParameters,
                _isGapByOffset,
                createArgumentSequences,
                _staticContext
            );
        });
    }

    private ISequence CallFunction<T>(
        FunctionValue<T, TNode> functionItem,
        FunctionSignature<T, TNode> functionCall,
        DynamicContext dynamicContext,
        ExecutionParameters<TNode> executionParameters,
        bool[] isGapByOffset,
        SequenceCallback[] createArgumentSequences,
        StaticContext<TNode>? staticContext) where T : ISequence
    {
        var argumentOffset = 0;
        var evaluatedArgs = isGapByOffset.Select(
            isGap => isGap ? null : createArgumentSequences[argumentOffset++](dynamicContext)
        ).ToArray();

        // Test if we have the correct arguments, and pre-convert the ones we can pre-convert
        var transformedArguments = TransformArgumentList(
            functionItem.ArgumentTypes.Cast<SequenceType>().ToArray(),
            evaluatedArgs,
            executionParameters,
            functionItem.Name
        );

        if (transformedArguments.Contains(null))
        {
            var res = functionItem.ApplyArguments(transformedArguments);
            return res;
        }

        var toReturn = functionCall(
            dynamicContext,
            executionParameters,
            staticContext,
            transformedArguments!
        );

        return ArgumentHelper<TNode>.PerformFunctionConversion(
            functionItem.ReturnType,
            toReturn,
            executionParameters,
            functionItem.Name,
            true
        );
    }

    public override void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        _staticContext = staticContext.Clone();

        base.PerformStaticEvaluation(staticContext);

        if (_functionReferenceExpression.CanBeStaticallyEvaluated)
        {
            var functionRefSequence = _functionReferenceExpression.EvaluateMaybeStatically(null, null);

            if (!functionRefSequence.IsSingleton()) throw Xpty0004;

            _functionReference = ValidateFunctionItem<ISequence>(functionRefSequence.First()!, _callArity);

            if (_functionReference.IsUpdating) IsUpdating = true;
        }
    }

    public static ISequence?[] TransformArgumentList(
        SequenceType[] argumentTypes,
        ISequence?[] argumentList,
        ExecutionParameters<TNode> executionParameters,
        string functionItem
    )
    {
        var transformedArguments = new List<ISequence?>();
        for (var i = 0; i < argumentList.Length; ++i)
        {
            if (argumentList[i] == null)
            {
                // This is the result of partial application, it will be inserted later
                transformedArguments.Add(null);
                continue;
            }

            var transformedArgument = ArgumentHelper<TNode>.PerformFunctionConversion(
                argumentTypes[i],
                argumentList[i],
                executionParameters,
                functionItem,
                false
            );
            transformedArguments.Add(transformedArgument);
        }

        return transformedArguments.ToArray();
    }

    private static FunctionValue<T, TNode> ValidateFunctionItem<T>(AbstractValue item, int callArity)
        where T : ISequence
    {
        if (!item.GetValueType().IsSubtypeOf(ValueType.Function))
            throw new XPathException(
                "XPTY0004",
                "Expected base expression to evaluate to a function item"
            );

        var functionItem = item.GetAs<FunctionValue<T, TNode>>();

        if (functionItem == null || functionItem.Arity != callArity)
            throw Xpty0004;

        return functionItem;
    }
}