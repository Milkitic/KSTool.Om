using System.Collections.ObjectModel;
using YamlDotNet.Serialization;

namespace KSTool.Om.Core.Models;

public class SoundCategory : ViewModelBase
{
    #region Configurable

    private string _name = "New Category";
    private int _defaultVolume = 100;
    private HitsoundCache? _selectedSound;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int DefaultVolume
    {
        get => _defaultVolume;
        set => this.RaiseAndSetIfChanged(ref _defaultVolume, value);
    }

    /// <summary>
    /// Relative paths
    /// </summary>
    public HashSet<string> SoundFileNames { get; set; } = new();

    #endregion

    [YamlIgnore]
    public ObservableCollection<HitsoundCache> Hitsounds { get; set; } = new();

    [YamlIgnore]
    public HitsoundCache? SelectedSound
    {
        get => _selectedSound;
        set => this.RaiseAndSetIfChanged(ref _selectedSound, value);
    }
}