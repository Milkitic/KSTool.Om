using System.Collections.ObjectModel;
using Coosu.Beatmap;
using YamlDotNet.Serialization;

namespace KSTool.Om.Core.Models;

public class ProjectDifficulty
{
    #region Configurable

    public string DifficultyName { get; set; } = "Unknown";
    public ObservableCollection<GroupTimingRule> GroupTimingRules { get; set; } = new();

    #endregion

    [YamlIgnore]
    public bool IsDifficultyLost => OsuFile == null;

    [YamlIgnore]
    public int Duration { get; set; }

    [YamlIgnore]
    public LocalOsuFile? OsuFile { get; set; }

    [YamlIgnore]
    public LocalOsuFile? GhostReferenceOsuFile { get; set; }
}