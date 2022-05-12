using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctions
{
    public static readonly BuiltinDeclarationType[] Declarations =
        BuiltInFunctionsNode.Declarations.Concat(BuiltInFunctionsString.Declarations).Concat(BuiltInFunctionsSequences.Declarations).ToArray();


    public static FunctionDefinitionType<ISequence> ContextItemAsFirstArgument(FunctionDefinitionType<ISequence> fn)
    {
        return (context, parameters, staticContext, args) =>
        {
            if (context?.ContextItem == null)
                throw new XPathException(
                    "XPDY0002: The function which was called depends on dynamic context, which is absent.");

            return fn(context, parameters, staticContext, new SingletonSequence(context.ContextItem));
        };
    }
}
