namespace KSTool.Om.Core.Models;

public class TimingRule
{
    public TimingRule(SoundCategory category, RangeInfo rangeInfo)
    {
        Category = category;
        TimingRange = rangeInfo.TimingRange;
        Volume = rangeInfo.Volume;
    }

    public SoundCategory Category { get; }
    public RangeValue<int> TimingRange { get; }
    public int Volume { get; }
}