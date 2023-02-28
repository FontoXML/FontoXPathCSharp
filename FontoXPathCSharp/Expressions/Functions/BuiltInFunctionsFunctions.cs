using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Functions;

public static class BuiltInFunctionsFunctions<TNode> where TNode : notnull
{
    private static readonly FunctionSignature<ISequence, TNode> FnFunctionLookup = (_, _, staticContext, sequences) =>
    {
        return ISequence.ZipSingleton(sequences, nameArityTuple =>
        {
            var name = nameArityTuple[0]!.GetAs<QNameValue>();
            var arity = nameArityTuple[1]!.GetAs<IntegerValue>();

            var functionProperties = staticContext?.LookupFunction(
                name.Value.NamespaceUri,
                name.Value.LocalName,
                (int)arity.Value
            );

            if (functionProperties == null) return SequenceFactory.CreateEmpty();

            var functionItem = new FunctionValue<ISequence, TNode>(
                functionProperties.ArgumentTypes,
                (int)arity.Value,
                name.Value.LocalName,
                name.Value.NamespaceUri!,
                functionProperties.ReturnType,
                functionProperties.CallFunction
            );

            return new SingletonSequence(functionItem);
        });
    };


    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(new[]
            {
                new ParameterType(ValueType.XsQName, SequenceMultiplicity.ExactlyOne),
                new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ZeroOrOne)
            },
            FnFunctionLookup, "function-lookup", BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Function, SequenceMultiplicity.ZeroOrOne))
    };
}