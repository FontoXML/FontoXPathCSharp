namespace FontoXPathCSharp;

public class Either<TL, TR>
{
    private readonly bool _isLeft;
    private readonly TL? _left;
    private readonly TR? _right;

    private Either(TL left)
    {
        _left = left;
        _isLeft = true;
    }

    private Either(TR right)
    {
        _right = right;
        _isLeft = false;
    }

    public bool IsLeft()
    {
        return _isLeft;
    }

    public TL AsLeft()
    {
        if (!_isLeft) throw new InvalidOperationException("Trying to get left either value which is not present");
        return _left!;
    }

    public TR AsRight()
    {
        if (_isLeft) throw new InvalidOperationException("Trying to get right either value which is not present");
        return _right!;
    }

    public T Match<T>(Func<TL, T> leftFunc, Func<TR, T> rightFunc)
    {
        return _isLeft ? leftFunc(_left!) : rightFunc(_right!);
    }

    public static implicit operator Either<TL, TR>(TL left)
    {
        return new Either<TL, TR>(left);
    }

    public static implicit operator Either<TL, TR>(TR right)
    {
        return new Either<TL, TR>(right);
    }
}