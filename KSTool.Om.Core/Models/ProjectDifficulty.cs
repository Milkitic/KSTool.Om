using System.Collections.ObjectModel;
using Coosu.Beatmap;
using YamlDotNet.Serialization;

namespace KSTool.Om.Core.Models;

public class ProjectDifficulty : ViewModelBase
{
    private ObservableCollection<TimingRule> _flattenTimingRules = new();

    #region Configurable

    public string DifficultyName { get; set; } = "Unknown";
    public List<GroupTimingRule> GroupTimingRules { get; set; } = new();

    #endregion

    [YamlIgnore]
    public ObservableCollection<TimingRule> FlattenTimingRules
    {
        get => _flattenTimingRules;
        set => this.RaiseAndSetIfChanged(ref _flattenTimingRules, value);
    }

    [YamlIgnore]
    public bool IsDifficultyLost => OsuFile == null;

    [YamlIgnore]
    public int Duration { get; set; }

    [YamlIgnore]
    public LocalOsuFile? OsuFile { get; set; }

    [YamlIgnore]
    public LocalOsuFile? GhostReferenceOsuFile { get; set; }

}