namespace FontoXPathCSharp;

public abstract class EvaluableExpression<T>
{
    private readonly T expression;

    public EvaluableExpression(T expression)
    {
        this.expression = expression;
    }

    public T Evaluate()
    {
        return expression;
    }
}