namespace FontoXPathCSharp.Sequences;

using Value;

public interface ISequence
{
    bool IsEmpty();
    bool IsSingleton();
    AbstractValue? First();
    int GetLength();
}