namespace FontoXPathCSharp;

public class FunctionSignature
{
    public DynamicContext DynamicContext;
    public ExecutionParameters ExecutionParameters;
    public StaticContext StaticContext;

    public FunctionSignature(DynamicContext dynamicContext, ExecutionParameters executionParameters,
        StaticContext staticContext)
    {
        DynamicContext = dynamicContext;
        ExecutionParameters = executionParameters;
        StaticContext = staticContext;
    }
}