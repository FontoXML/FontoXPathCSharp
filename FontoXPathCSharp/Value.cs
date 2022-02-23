namespace FontoXPathCSharp;

public enum ValueType
{
    XSBOOLEAN,
    XSSTRING,
    XSNUMERIC,
    NODE
}

public class Value
{
    public ValueType type;
    public object value;

    public Value(object value, ValueType type)
    {
        this.value = value;
        this.type = type;
    }
}