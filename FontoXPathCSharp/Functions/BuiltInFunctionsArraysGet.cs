using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Functions;

public class BuiltInFunctionsArraysGet<TNode> where TNode : notnull
{
    public static ISequence ArrayGet(
        ISequence arraySequence,
        ISequence positionSequence)
    {
        return positionSequence.MapAll(positions =>
        {
            var position = positions.First();
            var positionValue = position.GetAs<IntValue>().Value;
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