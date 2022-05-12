using System.Collections;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public class EmptySequence : ISequence
{
    public bool IsEmpty()
    {
        return true;
    }

    public bool IsSingleton()
    {
        return false;
    }

    public AbstractValue? First()
    {
        return null;
    }

    public AbstractValue[] GetAllValues()
    {
        return Array.Empty<AbstractValue>();
    }
    
    public int GetLength()
    {
        return 0;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public IEnumerator<AbstractValue> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return "<EmptySequence>[]";
    }
}