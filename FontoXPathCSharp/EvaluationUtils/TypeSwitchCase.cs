namespace FontoXPathCSharp.EvaluationUtils;

public class TypeSwitchCase<TReturn>
{
    // This construction allows to switch on types, making the resulting code a bit easier to work with.
    // typeActions is effectively the switch-case jump table.
    // The if check after it serves as a default case.
    private Dictionary<Type, Func<TReturn>> _actions;

    public TypeSwitchCase(IEnumerable<KeyValuePair<Type, Func<TReturn>>> actions)
    {
        _actions = new Dictionary<Type, Func<TReturn>>(actions);
    }

    public TReturn Run(Type typeCase)
    {
        if (!_actions.ContainsKey(typeCase))
        {
            throw new Exception("Type case for " + typeCase + " does not exist.");
        }
        return _actions[typeCase]();
    }
}