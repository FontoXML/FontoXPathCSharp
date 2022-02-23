namespace FontoXPathCSharp.Sequences;

public interface ISequence
{
    bool IsEmpty();
    bool IsSingleton();
    Value? First();
    int GetLength();
}