using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp;

public class DynamicContext
{
    public AbstractValue? ContextItem;
    public int ContextItemIndex;
    public ISequence ContextSequence;

    public DynamicContext(AbstractValue? contextItem, int contextItemIndex, ISequence contextSequence)
    {
        ContextItem = contextItem;
        ContextItemIndex = contextItemIndex;
    }

    public DynamicContext ScopeWithFocus(int contextItemIndex, AbstractValue? contextItem, ISequence? contextSequence)
    {
        return new DynamicContext(contextItem, contextItemIndex, contextSequence ?? ContextSequence);
    }

    public Iterator<DynamicContext> CreateSequenceIterator(ISequence contextSequence)
    {
        var i = 0;
        var iterator = contextSequence.GetValue();
        return hint => {
                var value = iterator(hint);
                return value.IsDone 
                    ? IteratorResult<DynamicContext>.Done() 
                    : IteratorResult<DynamicContext>.Ready(ScopeWithFocus(i++, value.Value, contextSequence));
        };
    }
}