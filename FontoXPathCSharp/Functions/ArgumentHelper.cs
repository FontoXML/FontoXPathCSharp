using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Functions;

public class ArgumentHelper
{
    public static ISequence PerformFunctionConversion<TNode>(
        SequenceType argumentType,
        ISequence argument,
        ExecutionParameters<TNode> executionParameters,
        string functionName,
        bool isReturn) where TNode : notnull
    {
        throw new NotImplementedException("PerformFunctionConversion is not implemented yet.");
    }
}