namespace FontoXPathCSharp.Expressions.Util;

public abstract class Result
{
    public bool Success { get; protected set; }
    public bool Failure => !Success;
}

public abstract class Result<T> : Result
{
    private T _data;

    protected Result(T data)
    {
        _data = data;
    }

    public T Data
    {
        get => Success
            ? _data
            : throw new Exception($"You can't access .{nameof(Data)} when .{nameof(Success)} is false");
        set => _data = value;
    }
}

public class SuccessResult : Result
{
    public SuccessResult()
    {
        Success = true;
    }
}

public class SuccessResult<T> : Result<T>
{
    public SuccessResult(T data) : base(data)
    {
        Success = true;
    }
}

public class ErrorResult : Result, IErrorResult
{
    public ErrorResult(string message) : this(message, null)
    {
    }

    public ErrorResult(string message, string? errorCode)
    {
        Message = message;
        Success = false;
        ErrorCode = errorCode;
    }

    public string Message { get; }
    public string? ErrorCode { get; }
}

public class ErrorResult<T> : Result<T>, IErrorResult
{
    public ErrorResult(string message, string? errorCode = null) : base(default!)
    {
        Message = message;
        Success = false;
        ErrorCode = errorCode;
    }

    public string Message { get; }
    public string? ErrorCode { get; }
}

internal interface IErrorResult
{
}