namespace FontoXPathCSharp;

public abstract class EvaluableExpression<T>
{
    private readonly T _expression;

    public EvaluableExpression(T expression)
    {
        _expression = expression;
    }

    public T Evaluate()
    {
        return _expression;
    }
}