namespace KSTool.Om.Core.Models;

public class TimingRule : ViewModelBase
{
    private RangeValue<int> _timingRange;
    private int? _volume;
    private SoundCategory _category;

    public TimingRule(SoundCategory category, RangeInfo rangeInfo)
    {
        Category = category;
        TimingRange = rangeInfo.TimingRange;
        Volume = rangeInfo.Volume;
    }

    public bool IsCategoryLost { get; set; }

    public SoundCategory Category
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    public RangeValue<int> TimingRange
    {
        get => _timingRange;
        set => this.RaiseAndSetIfChanged(ref _timingRange, value);
    }

    public int? Volume
    {
        get => _volume;
        set => this.RaiseAndSetIfChanged(ref _volume, value);
    }
}