namespace FontoXPathCSharp.Types;

public class CompilationOptions
{
    public CompilationOptions(bool allowUpdating, bool allowXQuery, bool debug, bool disableCache)
    {
        AllowUpdating = allowUpdating;
        AllowXQuery = allowXQuery;
        Debug = debug;
        DisableCache = disableCache;
    }

    public static CompilationOptions XPathMode =>
        new(allowXQuery: false, allowUpdating: false, disableCache: false, debug: false);

    public static CompilationOptions XQueryMode =>
        new(allowXQuery: true, allowUpdating: false, disableCache: false, debug: false);

    public static CompilationOptions XQueryUpdatingMode =>
        new(allowXQuery: true, allowUpdating: true, disableCache: false, debug: false);

    public bool AllowUpdating { get; }
    public bool AllowXQuery { get; }
    public bool Debug { get; }
    public bool DisableCache { get; }

    public void Deconstruct(out bool allowUpdating, out bool allowXQuery, out bool debug, out bool disableCache)
    {
        allowUpdating = AllowUpdating;
        allowXQuery = AllowXQuery;
        debug = Debug;
        disableCache = DisableCache;
    }
}