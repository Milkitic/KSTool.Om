namespace KSTool.Om.Core.Models;

public class SoundFile : ViewModelBase
{
    private string? _relativePath;
    private string? _baseFolder;

    private string _filePath = "";

    /// <summary>
    /// Should be full path
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    public bool IsFileLost { get; set; }

    public string GetRelativePath(string baseFolder)
    {
        if (_relativePath != null && baseFolder == _baseFolder) return _relativePath;
        _baseFolder = baseFolder;

        var relativePath = IOUtils.GetRelativePath(baseFolder, FilePath);
        return _relativePath = relativePath;
    }
}