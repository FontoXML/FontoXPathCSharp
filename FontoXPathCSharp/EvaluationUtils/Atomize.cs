using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.EvaluationUtils;

public class Atomize
{
    public static ISequence AtomizeSequence(ISequence sequence, ExecutionParameters parameters)
    {
        var done = false;
        var it = sequence.GetValue();
        Iterator<AbstractValue>? currentOutput = null;

        // Func<IteratorResult<AbstractValue>> result = () => {
        //     while (!done)
        //     {
        //         if (currentOutput == null)
        //         {
        //             var inputItem = it.Next(IterationHint.None);
        //             if (inputItem.IsDone)
        //             {
        //                 done = true;
        //                 break;
        //             }
        //
        //             ISequence outputSequence = AtomizeSingleValue(inputItem.Value, parameters);
        //             currentOutput = outputSequence.GetValue();
        //         }
        //
        //         var itemToOutput = currentOutput.Next(IterationHint.None);
        //         if (itemToOutput.IsDone)
        //         {
        //             currentOutput = null;
        //             continue;
        //         }
        //
        //         return itemToOutput;
        //     }
        // };
        //
        // return SequenceFactory.CreateFromIterator(result());
    }

    private static ISequence AtomizeSingleValue(AbstractValue inputItemValue, ExecutionParameters parameters)
    {
        throw new NotImplementedException();
    }
}