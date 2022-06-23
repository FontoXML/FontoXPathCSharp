namespace FontoXPathCSharp.EvaluationUtils;

public class TypeSwitchCase<TReturn> : Dictionary<Type, Func<TReturn?>>
{
    // This construction allows to switch on types, making the resulting code a bit easier to work with.
    // typeActions is effectively the switch-case jump table.
    // The if check after it serves as a default case.

    public TReturn? Run(Type typeCase)
    {
        if (!ContainsKey(typeCase)) throw new Exception("Type case for " + typeCase + " does not exist.");
        return this[typeCase]();
    }
}