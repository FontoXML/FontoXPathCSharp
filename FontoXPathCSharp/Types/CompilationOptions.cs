namespace FontoXPathCSharp.Types;

public class CompilationOptions
{
    public CompilationOptions(bool AllowUpdating, bool AllowXQuery, bool Debug, bool DisableCache)
    {
        this.AllowUpdating = AllowUpdating;
        this.AllowXQuery = AllowXQuery;
        this.Debug = Debug;
        this.DisableCache = DisableCache;
    }

    public static CompilationOptions XPathMode =>
        new(AllowXQuery: false, AllowUpdating: false, DisableCache: false, Debug: false);

    public static CompilationOptions XQueryMode =>
        new(AllowXQuery: true, AllowUpdating: false, DisableCache: false, Debug: false);

    public static CompilationOptions XQueryUpdatingMode =>
        new(AllowXQuery: true, AllowUpdating: true, DisableCache: false, Debug: false);

    public bool AllowUpdating { get; }
    public bool AllowXQuery { get; }
    public bool Debug { get; }
    public bool DisableCache { get; }

    public void Deconstruct(out bool AllowUpdating, out bool AllowXQuery, out bool Debug, out bool DisableCache)
    {
        AllowUpdating = this.AllowUpdating;
        AllowXQuery = this.AllowXQuery;
        Debug = this.Debug;
        DisableCache = this.DisableCache;
    }
}