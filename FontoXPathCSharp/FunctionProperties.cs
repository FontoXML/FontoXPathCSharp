using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp;

public record FunctionProperties(ParameterType[] ArgumentTypes, int Arity,
    FunctionSignature<ISequence> CallFunction, bool IsUpdating, string LocalName, string NamespaceUri,
    SequenceType ReturnType);