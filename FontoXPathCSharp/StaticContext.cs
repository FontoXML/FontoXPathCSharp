using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp;

public struct FunctionProperties
{
    public readonly int Arity;
    public readonly FunctionSignature<ISequence> CallFunction;
    public readonly string LocalName;

    public readonly string NamespaceUri;
    // TODO: add argument and return types

    public FunctionProperties(int arity, FunctionSignature<ISequence> callFunction, string localName,
        string namespaceUri)
    {
        Arity = arity;
        CallFunction = callFunction;
        LocalName = localName;
        NamespaceUri = namespaceUri;
    }
}

public class StaticContext
{
    public StaticContext Clone()
    {
        return new StaticContext();
    }

    public FunctionProperties? LookupFunction(string namespaceUri, string localName, int arity)
    {
        if (localName == "test")
        {
            return new FunctionProperties(arity, (context, parameters, staticContext, args) =>
            {
                Console.WriteLine("Called test function");
                return new EmptySequence();
            }, localName, namespaceUri);
        }

        return null;
    }
}