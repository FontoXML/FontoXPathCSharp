using System.IO;
using System.Text;

namespace XPathTest;

public static class TestFileSystem
{
    private static string CreateAssetPath(string assetPath)
    {
        var path = Path.Join("..", "..", "..", "assets", assetPath);
        path = Path.Combine(Directory.GetCurrentDirectory(), path);
        return path;
    }

    public static string[] ReadDir(string filepath)
    {
        return Directory.GetFiles(CreateAssetPath(filepath));
    }

    public static string ReadFile(string filepath)
    {
        return File.ReadAllText(CreateAssetPath(filepath), Encoding.UTF8);
    }

    public static void WriteFile(string filepath, string content, bool append = false)
    {
        if (append) content = ReadFile(filepath) + content;
        File.WriteAllText(CreateAssetPath(filepath), content, Encoding.UTF8);
    }

    public static bool FileExists(string filepath)
    {
        return File.Exists(CreateAssetPath(filepath));
    }
}