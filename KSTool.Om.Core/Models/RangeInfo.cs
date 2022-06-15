namespace KSTool.Om.Core.Models;

public class RangeInfo
{
    public int Volume { get; set; } = 100;
    public RangeValue<int> TimingRange { get; set; }
}