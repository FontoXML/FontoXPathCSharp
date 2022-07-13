using FontoXPathCSharp.Sequences;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class MapValue : AbstractValue
{
    public MapValue() : base(ValueType.Map)
    {
        KeyValuePairs = new List<KeyValuePair<AbstractValue, Func<ISequence>>>();
    }

    public MapValue(List<KeyValuePair<AbstractValue, Func<ISequence>>> keyValuePairs) : base(ValueType.Map)
    {
        KeyValuePairs = keyValuePairs;
    }

    public List<KeyValuePair<AbstractValue, Func<ISequence>>> KeyValuePairs { get; }
}