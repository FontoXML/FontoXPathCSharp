using FontoXPathCSharp;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestIteratorBackedSequence
{
    private static IteratorBackedSequence CreateTestSequence(int length, int? predictedLength = null)
    {
        var count = 0;
        return (IteratorBackedSequence) SequenceFactory.CreateFromIterator(_ =>
            count == length
                ? IteratorResult<AbstractValue>.Done()
                : IteratorResult<AbstractValue>.Ready(new IntValue(count++)), predictedLength);
    }

    [Fact]
    public void TestLengthEmptySequence()
    {
        Assert.Equal(0, CreateTestSequence(0).GetLength());
    }

    [Fact]
    public void TestLengthSingleItemSequence()
    {
        Assert.Equal(1, CreateTestSequence(1).GetLength());
    }
    
    [Fact]
    public void TestIsEmptyEmptySequence()
    {
        Assert.True(CreateTestSequence(0).IsEmpty());
    }

    [Fact]
    public void TestIsEmptySingleItemSequence()
    {
        Assert.False(CreateTestSequence(1).IsEmpty());
    }
}