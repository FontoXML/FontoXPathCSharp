using System.Security.Principal;

namespace FontoXPathCSharp.Expressions;

public class BuiltInTypeModels
{
    private BuiltInTypeModels()
    {
        
    }

    private static BuiltInTypeModels? _instance;

    public static BuiltInTypeModels Instance() => _instance ??= new BuiltInTypeModels();
}