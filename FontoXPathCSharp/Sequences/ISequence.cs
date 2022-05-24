using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Sequences;

public interface ISequence
{
    bool IsEmpty();
    bool IsSingleton();
    AbstractValue? First();
    int GetLength();

    bool GetEffectiveBooleanValue();
}