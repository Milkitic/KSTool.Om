using System.Collections.ObjectModel;
using YamlDotNet.Serialization;

namespace KSTool.Om.Core.Models;

public class GroupTimingRule
{
    #region Configurable

    public string PreferredCategory { get; set; } = "New Category";
    public ObservableCollection<RangeInfo> RangeInfos { get; set; } = new();

    #endregion

    [YamlIgnore]
    public bool IsCategoryLost { get; set; }

    [YamlIgnore]
    public SoundCategory? Category { get; set; }
}

public class RangeInfo
{
    public int Volume { get; set; } = 100;
    public RangeValue<int> TimingRange { get; set; }
}