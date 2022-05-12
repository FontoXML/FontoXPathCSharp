using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public interface ISequence : IEnumerable<AbstractValue>
{
    bool IsEmpty();
    bool IsSingleton();
    AbstractValue? First();
    AbstractValue[] GetAllValues();
    int GetLength();
}