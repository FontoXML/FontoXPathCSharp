namespace XPathTest.Caches;

public class DocumentsByPathCache : ResourceCache<string, string>
{
    public static DocumentsByPathCache Instance { get; } = new();

    protected override string Load(string key)
    {
        return TestFileSystem.ReadFile(key);
    }
}