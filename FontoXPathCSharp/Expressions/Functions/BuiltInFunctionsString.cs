using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Functions;

public static class BuiltInFunctionsString
{
    private static readonly FunctionSignature<ISequence> FnStringLength = (context, parameters, staticContext, args) =>
    {
        if (args.Length == 0)
        {
            return new SingletonSequence(new IntValue(0));
        }

        var stringValue = args[0].First()!.GetAs<StringValue>(ValueType.XsString)!.Value;

        return new SingletonSequence(new IntValue(stringValue.Length));
    };

    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[] {new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)},
            callFunction: FnStringLength, localName: "string-length",
            namespaceUri: "http://www.w3.org/2005/xpath-functions",
            returnType: new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)),

        new(Array.Empty<ParameterType>(),
            callFunction: BuiltInFunctions.ContextItemAsFirstArgument(FnStringLength), localName: "string-length",
            namespaceUri: "http://www.w3.org/2005/xpath-functions",
            returnType: new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)),
    };
}