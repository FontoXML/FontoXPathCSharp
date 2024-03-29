using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Functions;

public static class BuiltInFunctionsArraysGet<TNode> where TNode : notnull
{
    public static ISequence ArrayGet(
        ISequence arraySequence,
        ISequence positionSequence)
    {
        return positionSequence.MapAll(positions =>
        {
            var position = positions.First();
            var positionValue = (int)position.GetAs<IntegerValue>().Value;
            return arraySequence.MapAll(arrays =>
            {
                var array = arrays.First();
                var arrayValue = array.GetAs<ArrayValue<TNode>>();
                if (positionValue <= 0 || positionValue > arrayValue.Members.Count)
                    throw new XPathException("FOAY0001", "Array position out of bounds.");
                return arrayValue.Members[positionValue - 1]();
            });
        });
    }
}