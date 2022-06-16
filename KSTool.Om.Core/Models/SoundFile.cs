namespace KSTool.Om.Core.Models;

public class SoundFile : ViewModelBase
{
    public string? CachedRelativePath { get; private set; }
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
        if (CachedRelativePath != null && baseFolder == _baseFolder) return CachedRelativePath;
        _baseFolder = baseFolder;

        var relativePath = IOUtils.GetRelativePath(baseFolder, FilePath);
        return CachedRelativePath = relativePath;
    }
}