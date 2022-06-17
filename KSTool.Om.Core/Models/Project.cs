﻿using System.Collections.ObjectModel;
using System.Globalization;
using Coosu.Beatmap;
using Coosu.Beatmap.Sections.Event;
using Coosu.Beatmap.Sections.HitObject;
using CsvHelper;
using CsvHelper.Configuration;
using YamlDotNet.Serialization;

namespace KSTool.Om.Core.Models;

public class Project : ViewModelBase
{
    private SoundCategory? _selectedCategory;
    private HitsoundCache? _selectedHitsound;
    private EditorSettings _editorSettings;
    private ObservableCollection<HitsoundCache> _unusedHitsoundFiles = new();
    private ProjectDifficulty? _currentDifficulty;
    private const string CurrentProjectVersion = "2.0";

    #region Configurable

    [YamlMember]
    public string? KsProjectVersion { get; set; }

    [YamlMember]
    public string? TemplateCsvFile { get; set; }

    [YamlMember]
    public string ProjectName { get; set; } = "New Project";

    [YamlMember]
    public string OsuBeatmapDir { get; set; } = ".";

    [YamlMember]
    public ObservableCollection<SoundCategory> SoundCategories { get; private set; } = new();

    [YamlMember]
    public ObservableCollection<ProjectDifficulty> Difficulties { get; private set; } = new();


    [YamlMember]
    public EditorSettings EditorSettings
    {
        get => _editorSettings ??= new EditorSettings();
        set => _editorSettings = value;
    }

    #endregion

    [YamlIgnore]
    public Dictionary<int, HashSet<string>> Templates { get; set; } = new();

    [YamlIgnore]
    public Dictionary<string, HitsoundCache> HitsoundFiles { get; } = new();

    [YamlIgnore]
    public ICollection<HitsoundCache> HitsoundCaches =>
        EditorSettings.ShowUsedChecked ? HitsoundFiles.Values : UnusedHitsoundFiles;

    [YamlIgnore]
    public ObservableCollection<HitsoundCache> UnusedHitsoundFiles
    {
        get => _unusedHitsoundFiles;
        set => this.RaiseAndSetIfChanged(ref _unusedHitsoundFiles, value);
    }

    [YamlIgnore]
    public string? ProjectPath { get; set; }

    [YamlIgnore]
    public SoundCategory? SelectedCategory
    {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    [YamlIgnore]
    public ProjectDifficulty? CurrentDifficulty
    {
        get => _currentDifficulty;
        set => this.RaiseAndSetIfChanged(ref _currentDifficulty, value);
    }

    [YamlIgnore]
    public bool IsModified { get; set; }

    public void Save(string path)
    {
        EditorSettings.LastSelectedDifficulty = CurrentDifficulty?.DifficultyName;
        foreach (var soundCategoryVm in SoundCategories)
        {
            soundCategoryVm.SoundFileNames.Clear();
            foreach (var hitsoundCache in soundCategoryVm.Hitsounds)
            {
                var soundFile = hitsoundCache.SoundFile;
                if (soundFile.IsFileLost)
                {
                    soundCategoryVm.SoundFileNames.Add(soundFile.FilePath);
                }
                else
                {
                    soundCategoryVm.SoundFileNames.Add(soundFile.GetRelativePath(OsuBeatmapDir));
                }
            }
        }

        foreach (var projectDifficulty in Difficulties)
        {
            var grouped = projectDifficulty.FlattenTimingRules
                .GroupBy(k => k.Category.Name)
                .Select(k => new GroupTimingRule()
                {
                    PreferredCategory = k.Key,
                    RangeInfos = k.Select(o => new RangeInfo
                    {
                        TimingRange = o.TimingRange,
                        Volume = o.Volume
                    }).ToList()
                })
                .ToList();
            projectDifficulty.GroupTimingRules = grouped;
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

    public void AddSoundFileIntoSoundCategory(HitsoundCache hitsoundCache, SoundCategory soundCategory)
    {
        soundCategory.SoundFileNames.Add(hitsoundCache.SoundFile.GetRelativePath(OsuBeatmapDir));
        soundCategory.Hitsounds.Add(hitsoundCache);
    }

    public void RefreshShowHitsoundType()
    {
        OnPropertyChanged(nameof(HitsoundCaches));
    }

    public void ComputeUnusedHitsounds()
    {
        var all = SoundCategories
            .SelectMany(k => k.Hitsounds.Select(o => o.SoundFile.GetRelativePath(OsuBeatmapDir)))
            .ToHashSet();

        UnusedHitsoundFiles =
            new ObservableCollection<HitsoundCache>(HitsoundCaches.Where(k =>
                !all.Contains(k.SoundFile.GetRelativePath(OsuBeatmapDir))));
    }

    public async Task ExportCurrentDifficultyAsync()
    {
        if (CurrentDifficulty == null) throw new Exception("You haven't selected any difficulties!");
        if (string.IsNullOrWhiteSpace(TemplateCsvFile)) throw new Exception("You haven't selected any template files!");
        var osuFile = CurrentDifficulty.OsuFile;
        if (osuFile == null) throw new Exception("Osu file is lost");

        await Task.Run(() =>
        {
            // Clear current sounds
            osuFile.Events.Samples.Clear();
            foreach (var rawHitObject in osuFile.HitObjects.HitObjectList)
            {
                rawHitObject.SampleSet = ObjectSamplesetType.Auto;
                rawHitObject.AdditionSet = ObjectSamplesetType.Auto;
                rawHitObject.Hitsound = HitsoundType.Normal;
                rawHitObject.SampleVolume = 0;
                rawHitObject.FileName = null;
            }

            var flattenRules = CurrentDifficulty.FlattenTimingRules
                .Where(k => !k.IsCategoryLost)
                .ToList();

            var allObjects = osuFile.HitObjects.HitObjectList
                .GroupBy(k => k.Offset)
                .ToDictionary(k => k.Key, k => k.ToArray());
            var ghostObjects =
                CurrentDifficulty.GhostReferenceOsuFile?.HitObjects.HitObjectList
                    .GroupBy(k => k.Offset)
                    .ToDictionary(k => k.Key, k => k.ToArray())
                ?? new Dictionary<int, RawHitObject[]>();

            foreach (var (timing, templateFiles) in Templates)
            {
                // Get available sounds
                var hitsoundCaches = templateFiles
                    .Select(k =>
                    {
                        var success = HitsoundFiles.TryGetValue(k, out var cache);
                        return (isSuccess: success, cache);
                    })
                    .Where(k => k.isSuccess && !k.cache!.SoundFile.IsFileLost)
                    .Select(k => k.cache!)
                    .ToArray();
                var unhandledHitsoundCacheList = new List<HitsoundCache>(hitsoundCaches);

                // Get true object list to copy
                var existObjects = GetCurrentTimingObjects(allObjects, timing, ghostObjects);
                var unhandledObjectList = new List<RawHitObject>(existObjects);

                // Get available categories 
                var currentCategories = GetCurrentTimingRules(flattenRules, timing);

                // Copy by current timing rule if exist (suggestion)
                foreach (var hitsoundCache in hitsoundCaches)
                {
                    var relativePath = hitsoundCache.SoundFile.GetRelativePath(OsuBeatmapDir);
                    var timingRule =
                        currentCategories.FirstOrDefault(k => k.Category.SoundFileNames.Contains(relativePath));
                    if (timingRule != null)
                    {
                        ExecuteCopy(timingRule.Volume ?? timingRule.Category.DefaultVolume, timing,
                            hitsoundCache, unhandledObjectList,
                            unhandledHitsoundCacheList, osuFile.Events.Samples, true);
                    }
                }

                // Copy the the other things
                foreach (var hitsoundCache in unhandledHitsoundCacheList.ToArray())
                {
                    var relativePath = hitsoundCache.SoundFile.GetRelativePath(OsuBeatmapDir);
                    var volume = currentCategories.FirstOrDefault(k => k.Category.SoundFileNames.Contains(relativePath))
                        ?.Volume;
                    if (volume == null)
                    {
                        volume = SoundCategories.FirstOrDefault(k => k.SoundFileNames.Contains(relativePath))
                            ?.DefaultVolume;
                    }

                    ExecuteCopy(volume ?? 0, timing, hitsoundCache, unhandledObjectList,
                        unhandledHitsoundCacheList, osuFile.Events.Samples, false);
                }
            }

            osuFile.Metadata.Version += " (KS)";
            osuFile.WriteOsuFile(Path.Combine(OsuBeatmapDir, osuFile.GetPath(osuFile.Metadata.Version)));
        });
    }

    public static async Task<Project> LoadAsync(string projectPath)
    {
        var content = File.ReadAllText(projectPath);
        var projectVm = YamlConverter.DeserializeSettings<Project>(content);
        if (string.IsNullOrEmpty(projectVm.KsProjectVersion))
            throw new Exception("Invalid project file.");
        if (projectVm.KsProjectVersion != CurrentProjectVersion)
            throw new Exception("Unsupported project file version: " + projectVm.KsProjectVersion);

        projectVm.ProjectPath = projectPath;
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
        project.KsProjectVersion = CurrentProjectVersion;

        if (!string.IsNullOrWhiteSpace(project.TemplateCsvFile))
        {
            project.LoadTemplateFile(project.TemplateCsvFile);
        }

        var files = IOUtils.EnumerateFiles(project.OsuBeatmapDir, ".wav", ".mp3", ".ogg", ".osu");
        var ghostReferences = new Dictionary<string, LocalOsuFile>();

        foreach (var fileInfo in files)
        {
            // Difficulties
            if (fileInfo.Extension.Equals(".osu", StringComparison.OrdinalIgnoreCase))
            {
                var osuFile = await OsuFile.ReadFromFileAsync(fileInfo.FullName);
                var version = osuFile.Metadata.Version;

                if (version?.EndsWith(" (KS)") == true)
                    continue;
                if (version?.EndsWith(" (GHOST)") == true)
                {
                    ghostReferences.Add(fileInfo.FullName, osuFile);
                    continue;
                }

                var existDifficulty = project.Difficulties.FirstOrDefault(k => k.DifficultyName == version);

                if (existDifficulty == null)
                {
                    existDifficulty = new ProjectDifficulty
                    {
                        DifficultyName = version
                    };
                    project.Difficulties.Add(existDifficulty);
                }

                existDifficulty.OsuFile = osuFile;
                existDifficulty.Duration = (int)osuFile.HitObjects.MaxTime;
            }
            else // Hitsound cache
            {
                var hitsoundCache = await HitsoundCache.CreateAsync(AudioManager.Instance.Engine.WaveFormat,
                    fileInfo.FullName);
                project.HitsoundFiles.Add(hitsoundCache.SoundFile.GetRelativePath(project.OsuBeatmapDir),
                    hitsoundCache);
            }
        }

        // Ghost references
        foreach (var ghostReference in ghostReferences)
        {
            var ghostVersion = ghostReference.Value.Metadata.Version!;
            var version = ghostVersion.Substring(0, ghostVersion.Length - 8);

            var existDifficulty = project.Difficulties.FirstOrDefault(k => k.DifficultyName == version);
            if (existDifficulty == null) continue;

            existDifficulty.GhostReferenceOsuFile = ghostReference.Value;
        }

        foreach (var soundCategoryVm in project.SoundCategories)
        {
            foreach (var soundFileRelative in soundCategoryVm.SoundFileNames)
            {
                if (project.HitsoundFiles.TryGetValue(soundFileRelative, out var cache))
                {
                    soundCategoryVm.Hitsounds.Add(cache);
                }
                else
                {
                    soundCategoryVm.Hitsounds.Add(HitsoundCache.CreateLost(soundFileRelative));
                }
            }
        }

        foreach (var projectDifficulty in project.Difficulties)
        {
            var flattenRules = new List<TimingRule>();
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

                flattenRules.AddRange(groupTimingRule.RangeInfos
                    .Select(range => new TimingRule(groupTimingRule.Category!, range)
                    {
                        IsCategoryLost = groupTimingRule.IsCategoryLost
                    })
                );
            }

            projectDifficulty.FlattenTimingRules = new ObservableCollection<TimingRule>(flattenRules);
        }

        project.ComputeUnusedHitsounds();

        project.CurrentDifficulty =
            project.Difficulties.FirstOrDefault(k => k.DifficultyName == project.EditorSettings.LastSelectedDifficulty) ??
            project.Difficulties.FirstOrDefault();
    }

    private HashSet<TimingRule> GetCurrentTimingRules(List<TimingRule> flattenRules, int timing)
    {
        var categories = flattenRules
            .Where(k => k.TimingRange.Start <= timing && k.TimingRange.End >= timing)
            .ToHashSet();
        return categories;
    }

    private RawHitObject[] GetCurrentTimingObjects(Dictionary<int, RawHitObject[]> allObjects, int timing,
        Dictionary<int, RawHitObject[]> ghostObjects)
    {
        if (!allObjects.TryGetValue(timing, out var existObjects))
        {
            existObjects = Array.Empty<RawHitObject>();
        }

        if (!ghostObjects.TryGetValue(timing, out var ghosts))
        {
            ghosts = Array.Empty<RawHitObject>();
        }

        if (ghosts.Length > 0)
        {
            existObjects = existObjects.Where(k => !ghosts.Contains(k, HitObjectComparer.Instance)).ToArray();
        }

        return existObjects;
    }

    private void ExecuteCopy(int volume, int timing, HitsoundCache hitsoundCache,
        List<RawHitObject> unhandledObjectList, List<HitsoundCache> unhandledHitsoundFileList,
        List<StoryboardSampleData> samples, bool isCategoryScope)
    {
        if (isCategoryScope)
        {
            // 优先复制在类别列表中的建议规则，如果不在则延后复制
            if (unhandledObjectList.Count == 0) return;
            GeneralCopy(volume, hitsoundCache, unhandledObjectList, unhandledHitsoundFileList);
            return;
        }

        if (unhandledObjectList.Count == 0) // 没有物件用来复制了，但是存在待复制的列表，复制进SB音效中
        {
            samples.Add(new StoryboardSampleData
            {
                Filename = hitsoundCache.SoundFile.GetRelativePath(OsuBeatmapDir),
                Volume = (byte)(volume == 0 ? 100 : volume),
                Offset = timing
            });
            unhandledHitsoundFileList.Remove(hitsoundCache);
        }
        else
        {
            GeneralCopy(volume, hitsoundCache, unhandledObjectList, unhandledHitsoundFileList);
        }
    }

    private void GeneralCopy(int volume, HitsoundCache hitsoundCache, List<RawHitObject> unhandledObjectList,
        List<HitsoundCache> unhandledHitsoundFileList)
    {
        var rawHitObject = unhandledObjectList[0];
        unhandledObjectList.RemoveAt(0);

        rawHitObject.FileName = hitsoundCache.SoundFile.GetRelativePath(OsuBeatmapDir);
        rawHitObject.SampleVolume = (byte)volume;
        unhandledHitsoundFileList.Remove(hitsoundCache);
    }
}

public class EditorSettings : ViewModelBase
{
    private bool _showUsedChecked;

    public bool ShowUsedChecked
    {
        get => _showUsedChecked;
        set => this.RaiseAndSetIfChanged(ref _showUsedChecked, value);
    }

    public string? LastSelectedDifficulty { get; set; }
}