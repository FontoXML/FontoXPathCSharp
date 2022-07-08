using System.IO;
using System.Text;

namespace XPathTest;

public static class TestFileSystem
{
    private static string CreateAssetPath(string assetPath)
    {
        return Path.Join("..", "..", "..", "assets", assetPath);
    }

    public static string[] ReadDir(string filepath)
    {
        return Directory.GetFiles(CreateAssetPath(filepath));
    }

    public static string ReadFile(string filepath)
    {
        return File.ReadAllText(CreateAssetPath(filepath), Encoding.UTF8);
    }

    public static void WriteFileSync(string filepath, string content)
    {
        File.WriteAllText(CreateAssetPath(filepath), content, Encoding.UTF8);
    }

    public static bool FileExists(string filepath)
    {
        return File.Exists(CreateAssetPath(filepath));
    }
}