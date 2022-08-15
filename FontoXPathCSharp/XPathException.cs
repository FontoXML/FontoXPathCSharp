namespace FontoXPathCSharp;

public class XPathException : Exception
{
    public readonly string ErrorCode;
    
    public XPathException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}