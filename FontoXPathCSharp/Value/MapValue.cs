using System.Collections;
using FontoXPathCSharp.Sequences;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class MapValue : AbstractValue
{
    private List<KeyValuePair<AbstractValue, Func<ISequence>>> _keyValuePairs;

    public MapValue() : base(ValueType.Map)
    {
        _keyValuePairs = new List<KeyValuePair<AbstractValue, Func<ISequence>>>();
    }

    public MapValue(List<KeyValuePair<AbstractValue, Func<ISequence>>> keyValuePairs) : base(ValueType.Map)
    {
        _keyValuePairs = keyValuePairs;
    }

    public List<KeyValuePair<AbstractValue, Func<ISequence>>> KeyValuePairs => _keyValuePairs;
}