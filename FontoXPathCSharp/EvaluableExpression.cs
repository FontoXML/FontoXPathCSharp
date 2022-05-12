namespace FontoXPathCSharp;

public abstract class EvaluableExpression<T>
{
    private readonly T _expression;

    protected EvaluableExpression(T expression)
    {
        _expression = expression;
    }

    public T Evaluate()
    {
        return _expression;
    }
}