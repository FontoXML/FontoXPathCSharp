using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctions<TNode>
{
    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
        BuiltInFunctionsNode<TNode>.Declarations
            .Concat(BuiltInFunctionsString<TNode>.Declarations)
            .Concat(BuiltInFunctionsSequences<TNode>.Declarations)
            .Concat(BuiltInFunctionsBoolean<TNode>.Declarations)
            .Concat(BuiltInFunctionsFunctions<TNode>.Declarations)
            .Concat(BuiltInFunctionsQName<TNode>.Declarations)
            .Concat(BuiltInFunctionsDataTypeConstructors<TNode>.Declarations)
            .Concat(BuiltInFunctionsNumeric<TNode>.Declarations).ToArray();


    public static FunctionSignature<ISequence, TNode> ContextItemAsFirstArgument(FunctionSignature<ISequence, TNode> fn)
    {
        return (context, parameters, staticContext, args) =>
        {
            if (context?.ContextItem == null)
                throw new XPathException(
                    "XPDY0002: The function which was called depends on dynamic context, which is absent.");

            return fn(context, parameters, staticContext, SequenceFactory.CreateFromValue(context.ContextItem));
        };
    }
}