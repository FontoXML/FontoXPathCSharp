using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Functions;

public class BuiltInFunctionsContext<TNode> where TNode : notnull
{
    private static readonly FunctionSignature<ISequence, TNode> FnLast =
        (dynamicContext, _, _, _) =>
        {
            if (dynamicContext?.ContextItem == null)
                throw new XPathException("XPDY0002",
                    "The fn:last() function depends on dynamic context, which is absent.");

            var done = false;
            return SequenceFactory.CreateFromIterator(_ =>
            {
                if (done) return IteratorResult<AbstractValue>.Done();

                var length = dynamicContext.ContextSequence.GetLength();
                done = true;
                return IteratorResult<AbstractValue>.Ready(AtomicValue.Create(length, ValueType.XsInteger));
            });
        };


    private static readonly FunctionSignature<ISequence, TNode> FnPosition =
        (dynamicContext, _, _, _) =>
        {
            if (dynamicContext?.ContextItem == null)
                throw new XPathException("XPDY0002",
                    "The fn:position() function depends on dynamic context, which is absent.");

            return SequenceFactory.CreateFromValue(
                AtomicValue.Create(
                    dynamicContext.ContextItemIndex + 1,
                    ValueType.XsInteger)
            );
        };


    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(Array.Empty<ParameterType>(),
            FnLast,
            "last",
            BuiltInNamespaceUris.FunctionsNamespaceUri.GetUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)
        ),

        new(Array.Empty<ParameterType>(),
            FnPosition,
            "position",
            BuiltInNamespaceUris.FunctionsNamespaceUri.GetUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)
        )
    };
}