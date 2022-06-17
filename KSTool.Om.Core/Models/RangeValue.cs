namespace KSTool.Om.Core.Models;

public struct RangeValue<T> : IComparable<RangeValue<T>>, IComparable
    where T : IComparable<T>
{
    public RangeValue(T start, T end) : this()
    {
        Start = start;
        End = end;
    }

    public T Start { get; set; }
    public T End { get; set; }

    public override string ToString()
    {
        return $"{{{Start} - {End}}}";
    }

    public int CompareTo(object? obj)
    {
        if (obj is RangeValue<T> rv)
            return this.CompareTo(rv);
        return -1;
    }

    public int CompareTo(RangeValue<T> other)
    {
        var startComparison = Start.CompareTo(other.Start);
        if (startComparison != 0) return startComparison;
        return End.CompareTo(other.End);
    }
}