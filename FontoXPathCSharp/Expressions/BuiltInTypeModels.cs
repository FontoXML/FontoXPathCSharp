namespace FontoXPathCSharp.Expressions;

public class BuiltInTypeModels
{
    private static BuiltInTypeModels? _instance;

    private BuiltInTypeModels()
    {
    }

    public static BuiltInTypeModels Instance()
    {
        return _instance ??= new BuiltInTypeModels();
    }
}