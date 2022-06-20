using System.Collections;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Sequences;

internal class SingletonSequence : ISequence
{
    private readonly AbstractValue _onlyValue;

    public SingletonSequence(AbstractValue onlyValue)
    {
        _onlyValue = onlyValue;
    }

    public IEnumerator<AbstractValue> GetEnumerator()
    {
        return new[] { _onlyValue }.ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool IsEmpty()
    {
        return false;
    }

    public bool IsSingleton()
    {
        return true;
    }

    public AbstractValue First()
    {
        return _onlyValue;
    }

    public AbstractValue[] GetAllValues()
    {
        return new[] { _onlyValue };
    }

    public int GetLength()
    {
        return 1;
    }

    public Iterator<AbstractValue> GetValue()
    {
        throw new NotImplementedException();
    }

    public ISequence Filter(Func<AbstractValue, int, ISequence, bool> callback)
    {
        throw new NotImplementedException();
    }

    public ISequence Map(Func<AbstractValue, int, ISequence, AbstractValue> callback)
    {
        throw new NotImplementedException();
    }

    public ISequence MapAll(Func<AbstractValue[], ISequence> allvalues)
    {
        throw new NotImplementedException();
    }

    public bool GetEffectiveBooleanValue()
    {
        if (SubtypeUtils.IsSubtypeOf(_onlyValue.GetValueType(), ValueType.Node))
        {
            return true;
        }

        if (SubtypeUtils.IsSubtypeOf(_onlyValue.GetValueType(), ValueType.XsString))
        {
            return _onlyValue.GetAs<StringValue>(ValueType.XsString).Value.Length > 0;
        }
        if (SubtypeUtils.IsSubtypeOf(_onlyValue.GetValueType(), ValueType.XsAnyUri))
        {
            return _onlyValue.GetAs<StringValue>(ValueType.XsAnyUri).Value.Length > 0;
        }
        if (SubtypeUtils.IsSubtypeOf(_onlyValue.GetValueType(), ValueType.XsUntypedAtomic))
        {
            return _onlyValue.GetAs<StringValue>(ValueType.XsUntypedAtomic).Value.Length > 0;
        }

        if (SubtypeUtils.IsSubtypeOf(_onlyValue.GetValueType(), ValueType.XsNumeric)) {
            return _onlyValue.GetAs<IntValue>(ValueType.XsNumeric).Value > 0;
        }

        throw new Exception("FORG0006");
    }
}