using System.IO;
using System.Text;

namespace XPathTest;

public static class TestFileSystem
{
    private static string CreateAssetPath(string assetPath)
    {
        return Path.Join("..", "..", "..", Path.Join("assets", assetPath));
    }

    public static string[] ReadDir(string dirPath)
    {
        return Directory.GetFiles(CreateAssetPath(dirPath));
    }

    public static string ReadFile(string filePath)
    {
        return File.ReadAllText(CreateAssetPath(filePath), Encoding.UTF8);
    }

    public static void WriteFileSync(string filePath, string content)
    {
        File.WriteAllText(CreateAssetPath(filePath), content, Encoding.UTF8);
    }
}