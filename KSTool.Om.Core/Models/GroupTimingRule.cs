using System.Collections.ObjectModel;
using YamlDotNet.Serialization;

namespace KSTool.Om.Core.Models;

public class GroupTimingRule
{
    #region Configurable

    public string PreferredCategory { get; set; } = "New Category";
    public ObservableCollection<RangeValue<int>> TimingRanges { get; set; } = new();

    #endregion

    [YamlIgnore]
    public bool IsCategoryLost { get; set; }

    [YamlIgnore]
    public SoundCategory? Category { get; set; }
}