public struct ParseResult<T>
{
    bool Success;
    int Offset;
    string[] Expected;
    bool Fatal;
}

public interface Parser<T>
{
    public ParseResult<T> Parse(string input, int offset);
}

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello world");
    }
}
