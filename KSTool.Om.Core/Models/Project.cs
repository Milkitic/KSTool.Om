using System.Collections.ObjectModel;
using System.Globalization;
using Coosu.Beatmap;
using CsvHelper;
using CsvHelper.Configuration;
using Milki.Extensions.MixPlayer.NAudioExtensions;
using YamlDotNet.Serialization;

namespace KSTool.Om.Core.Models;

public class Project : ViewModelBase
{
    #region Configurable

    public string KsProjectVersion { get; set; } = "2.0";

    public string TemplateCsvFile { get; set; } = "";

    public string ProjectName { get; set; } = "New Project";

    public string OsuBeatmapDir { get; set; } = ".";

    [YamlMember]
    public ObservableCollection<SoundCategory> SoundCategories { get; private set; } = new();

    [YamlMember]
    public ObservableCollection<ProjectDifficulty> Difficulties { get; private set; } = new();

    #endregion

    [YamlIgnore]
    public Dictionary<int, HashSet<string>> Templates { get; set; } = new();

    [YamlIgnore]
    public Dictionary<string, HitsoundCache> HitsoundFiles { get; } = new();

    [YamlIgnore]
    public string? ProjectPath { get; set; }

    [YamlIgnore]
    public AudioPlaybackEngine? Engine { get; set; }

    public void Save(string path)
    {
        foreach (var soundCategoryVm in SoundCategories)
        {
            soundCategoryVm.SoundFiles.Clear();
            foreach (var soundFileVm in soundCategoryVm.SoundFileVms)
            {
                if (soundFileVm.IsFileLost)
                {
                    soundCategoryVm.SoundFiles.Add(soundFileVm.FilePath);
                }
                else
                {
                    soundCategoryVm.SoundFiles.Add(soundFileVm.GetRelativePath(OsuBeatmapDir));
                }
            }
        }

        var str = YamlConverter.SerializeSettings(this);
        File.WriteAllText(path, str);
    }

    public void LoadTemplateFile(string templateFile)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
        };
        using var reader = new StreamReader(templateFile);
        using var csv = new CsvReader(reader, config);
        foreach (var templateObject in csv.GetRecords<TemplateObject>())
        {
            if (!Templates.TryGetValue(templateObject.Timing, out var hashSet))
            {
                hashSet = new HashSet<string>();
                Templates.Add(templateObject.Timing, hashSet);
            }

            hashSet.Add(templateObject.RelativePath);
        }

        TemplateCsvFile = templateFile;
    }

    public void AddSoundFileIntoSoundCategory(SoundFile soundFile, SoundCategory soundCategory)
    {
        soundCategory.SoundFiles.Add(soundFile.GetRelativePath(OsuBeatmapDir));
        soundCategory.SoundFileVms.Add(soundFile);
    }

    public static async Task<Project> LoadAsync(string projectPath)
    {
        var content = File.ReadAllText(projectPath);
        var projectVm = YamlConverter.DeserializeSettings<Project>(content);
        if (string.IsNullOrEmpty(projectVm.KsProjectVersion))
            throw new Exception("Invalid project file.");
        if (projectVm.KsProjectVersion != "2.0")
            throw new Exception("Unsupported project file version: " + projectVm.KsProjectVersion);

        await InitializeProjectAsync(projectVm);
        return projectVm;
    }

    public static async Task<Project> CreateNewAsync(string projectName, string beatmapDir)
    {
        var projectVm = new Project
        {
            OsuBeatmapDir = beatmapDir,
            ProjectName = projectName,
        };
        await InitializeProjectAsync(projectVm);
        return projectVm;
    }

    private static async Task InitializeProjectAsync(Project project)
    {
        if (!string.IsNullOrEmpty(project.TemplateCsvFile))
        {
            project.LoadTemplateFile(project.TemplateCsvFile);
        }

        project.Engine = new AudioPlaybackEngine();

        var soundFiles = IOUtils.EnumerateFiles(project.OsuBeatmapDir, ".wav", ".ogg", ".osu");
        foreach (var fileInfo in soundFiles)
        {
            if (fileInfo.Extension.Equals(".osu", StringComparison.OrdinalIgnoreCase))
            {
                var osuFile = await OsuFile.ReadFromFileAsync(fileInfo.FullName);
                var version = osuFile.Metadata.Version;
                var exist = project.Difficulties.FirstOrDefault(k => k.Name == version);

                if (exist == null)
                {
                    exist = new ProjectDifficulty
                    {
                        Name = version
                    };
                    project.Difficulties.Add(exist);
                }

                exist.OsuFile = osuFile;
                exist.Duration = (int)osuFile.HitObjects.MaxTime;
            }
            else
            {
                var hitsoundCache = await HitsoundCache.CreateAsync(project.Engine.WaveFormat,
                    fileInfo.FullName);
                project.HitsoundFiles.Add(hitsoundCache.SoundFile.GetRelativePath(project.OsuBeatmapDir),
                    hitsoundCache);
            }
        }

        foreach (var soundCategoryVm in project.SoundCategories)
        {
            foreach (var soundFileRelative in soundCategoryVm.SoundFiles)
            {
                if (project.HitsoundFiles.TryGetValue(soundFileRelative, out var cache))
                {
                    soundCategoryVm.SoundFileVms.Add(cache.SoundFile);
                }
                else
                {
                    soundCategoryVm.SoundFileVms.Add(new SoundFile
                    {
                        FilePath = soundFileRelative,
                        IsFileLost = true
                    });
                }
            }
        }
        
        foreach (var projectDifficulty in project.Difficulties)
        {
            foreach (var groupTimingRule in projectDifficulty.GroupTimingRules)
            {
                var categoryInstance =
                    project.SoundCategories.FirstOrDefault(k => k.Name == groupTimingRule.PreferredCategory);
                if (categoryInstance == null)
                {
                    groupTimingRule.IsCategoryLost = true;
                }
                else
                {
                    groupTimingRule.Category = categoryInstance;
                }
            }
        }
    }
}