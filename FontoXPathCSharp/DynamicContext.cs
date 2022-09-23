using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp;

public class DynamicContext
{
    public AbstractValue? ContextItem;
    public int ContextItemIndex;
    public ISequence ContextSequence;
    public Dictionary<string, Func<ISequence>> VariableBindings;

    public DynamicContext(AbstractValue? contextItem, int contextItemIndex, ISequence contextSequence,
        Dictionary<string, Func<ISequence>> variableBindings)
    {
        ContextItem = contextItem;
        ContextItemIndex = contextItemIndex;
        ContextSequence = contextSequence;
        VariableBindings = variableBindings;
    }

    public DynamicContext ScopeWithFocus(int contextItemIndex, AbstractValue? contextItem, ISequence? contextSequence)
    {
        return new DynamicContext(contextItem, contextItemIndex, contextSequence ?? ContextSequence, VariableBindings);
    }

    public Iterator<DynamicContext> CreateSequenceIterator(ISequence contextSequence)
    {
        var i = 0;
        var iterator = contextSequence.GetValue();
        return hint =>
        {
            var value = iterator(hint);
            return value.IsDone
                ? IteratorResult<DynamicContext>.Done()
                : IteratorResult<DynamicContext>.Ready(ScopeWithFocus(i++, value.Value, contextSequence));
        };
    }

    public DynamicContext ScopeWithVariableBindings(Dictionary<string, Func<ISequence>> variableBindings)
    {
        return new DynamicContext(
            ContextItem,
            ContextItemIndex,
            ContextSequence,
            variableBindings
                .Union(VariableBindings)
                .ToDictionary(a => a.Key, b => b.Value)
        );
    }
}