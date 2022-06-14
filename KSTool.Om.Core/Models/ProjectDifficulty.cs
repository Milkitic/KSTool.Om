using System.Collections.ObjectModel;
using Coosu.Beatmap;
using YamlDotNet.Serialization;

namespace KSTool.Om.Core.Models;

public class ProjectDifficulty
{
    #region Configurable

    public string Name { get; set; } = "Unknown";
    public ObservableCollection<GroupTimingRule> GroupTimingRules { get; set; } = new();

    #endregion

    [YamlIgnore]
    public bool IsDifficultyLost => OsuFile == null;

    [YamlIgnore]
    public int Duration { get; set; }

    [YamlIgnore]
    public OsuFile? OsuFile { get; set; }
}