using FontoXPathCSharp.Value;

namespace FontoXPathCSharp;

public class DynamicContext
{
    public AbstractValue? ContextItem;
    public int ContextItemIndex;

    public DynamicContext(AbstractValue? contextItem, int contextItemIndex)
    {
        ContextItem = contextItem;
        ContextItemIndex = contextItemIndex;
    }
}