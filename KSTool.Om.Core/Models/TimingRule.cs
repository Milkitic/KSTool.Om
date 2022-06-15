namespace KSTool.Om.Core.Models;

public class TimingRule
{
    public TimingRule(SoundCategory category, RangeValue<int> timingRange)
    {
        Category = category;
        TimingRange = timingRange;
    }

    public SoundCategory Category { get; }
    public RangeValue<int> TimingRange { get; }
}