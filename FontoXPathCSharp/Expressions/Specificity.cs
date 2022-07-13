namespace FontoXPathCSharp.Expressions;

public enum SpecificityKind
{
    Attribute,
    External,
    NodeName,
    NodeType,
    Universal
}

public class Specificity : IComparable<Specificity>
{
    private static readonly SpecificityKind[] _specificityDimensions = Enum.GetValues<SpecificityKind>();

    private readonly Dictionary<SpecificityKind, int> _counts;

    public Specificity(Dictionary<SpecificityKind, int>? countsByKind = null)
    {
        countsByKind ??= new Dictionary<SpecificityKind, int>();
        _counts = _specificityDimensions
            .Select(specificityKind =>
                (specificityKind, countsByKind.ContainsKey(specificityKind) ? countsByKind[specificityKind] : 0))
            .ToDictionary(e => e.Item1, e => e.Item2);

        //You can't pass in invalid specificity kinds with this implementation, so the check after this was not needed.
    }


    public int CompareTo(Specificity? other)
    {
        if (other == null) return 1;
        foreach (var specificityDim in _specificityDimensions)
        {
            if (other._counts[specificityDim] < _counts[specificityDim]) return 1;

            if (other._counts[specificityDim] > _counts[specificityDim]) return -1;
        }

        return 0;
    }

    public Specificity Add(Specificity other)
    {
        var sum = _specificityDimensions.Aggregate(new Dictionary<SpecificityKind, int>(),
            (countsByKind, specificityKind) =>
            {
                countsByKind[specificityKind] = _counts[specificityKind] + other._counts[specificityKind];
                return countsByKind;
            });

        return new Specificity(sum);
    }
}