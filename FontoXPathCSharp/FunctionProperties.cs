using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp;

public record FunctionProperties(ParameterType[] ArgumentTypes, int Arity, FunctionDefinitionType<ISequence> CallFunction, bool IsUpdating, string LocalName, string NamespaceUri, SequenceType ReturnType);
