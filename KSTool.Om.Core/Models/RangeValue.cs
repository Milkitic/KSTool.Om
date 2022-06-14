namespace KSTool.Om.Core.Models;

public struct RangeValue<T>
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
}