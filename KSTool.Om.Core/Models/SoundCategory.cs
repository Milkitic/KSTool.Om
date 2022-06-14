using System.Collections.ObjectModel;
using YamlDotNet.Serialization;

namespace KSTool.Om.Core.Models;

public class SoundCategory : ViewModelBase
{
    #region Configurable

    private string _name = "New Category";
    private int _volume = 100;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int Volume
    {
        get => _volume;
        set => this.RaiseAndSetIfChanged(ref _volume, value);
    }

    /// <summary>
    /// Relative paths
    /// </summary>
    public HashSet<string> SoundFiles { get; set; } = new();

    #endregion

    [YamlIgnore]
    public ObservableCollection<SoundFile> SoundFileVms { get; set; } = new();
}