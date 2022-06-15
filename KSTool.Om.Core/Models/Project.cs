using System.Collections.ObjectModel;
using System.Globalization;
using Coosu.Beatmap;
using Coosu.Beatmap.Sections.Event;
using Coosu.Beatmap.Sections.HitObject;
using CsvHelper;
using CsvHelper.Configuration;
using Milki.Extensions.MixPlayer.NAudioExtensions;
using YamlDotNet.Serialization;

namespace KSTool.Om.Core.Models;

public class Project : ViewModelBase
{
    private const string CurrentProjectVersion = "2.0";

    #region Configurable

    public string? KsProjectVersion { get; set; }

    public string? TemplateCsvFile { get; set; }

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

    [YamlIgnore]
    public ProjectDifficulty? CurrentDifficulty { get; set; }

    public void Save(string path)
    {
        foreach (var soundCategoryVm in SoundCategories)
        {
            soundCategoryVm.SoundFileNames.Clear();
            foreach (var soundFileVm in soundCategoryVm.SoundFiles)
            {
                if (soundFileVm.IsFileLost)
                {
                    soundCategoryVm.SoundFileNames.Add(soundFileVm.FilePath);
                }
                else
                {
                    soundCategoryVm.SoundFileNames.Add(soundFileVm.GetRelativePath(OsuBeatmapDir));
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
        soundCategory.SoundFileNames.Add(soundFile.GetRelativePath(OsuBeatmapDir));
        soundCategory.SoundFiles.Add(soundFile);
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

            var flattenRules = new List<TimingRule>();
            foreach (var timingGroup in CurrentDifficulty.GroupTimingRules.Where(k => !k.IsCategoryLost))
            {
                flattenRules.AddRange(timingGroup.RangeInfos.Select(range => new TimingRule(timingGroup.Category!, range)));
            }

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
                var hitsoundCaches = templateFiles
                    .Select(k =>
                    {
                        var success = HitsoundFiles.TryGetValue(k, out var cache);
                        if (success && cache!.SoundFile.IsFileLost)
                        {
                            return (isSuccess: false, cache);
                        }

                        return (isSuccess: success, cache);
                    })
                    .Where(k => k.isSuccess && !k.cache!.SoundFile.IsFileLost)
                    .Select(k => k.cache!)
                    .ToArray();
                var unhandledHitsoundCacheList = new List<HitsoundCache>(hitsoundCaches);

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

                var unhandledObjectList = new List<RawHitObject>(existObjects);

                var currentCategories = GetCurrentTimingRules(flattenRules, timing);
                foreach (var hitsoundCache in hitsoundCaches)
                {
                    var relativePath = hitsoundCache.SoundFile.GetRelativePath(OsuBeatmapDir);
                    var timingRule = currentCategories.FirstOrDefault(k => k.Category.SoundFileNames.Contains(relativePath));
                    if (timingRule != null)
                    {
                        ExecuteCopy(timingRule.Volume, timing, hitsoundCache, unhandledObjectList,
                            unhandledHitsoundCacheList, osuFile.Events.Samples, true);
                    }
                }

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


        static HashSet<TimingRule> GetCurrentTimingRules(List<TimingRule> flattenRules, int timing)
        {
            var categories = flattenRules
                .Where(k => k.TimingRange.Start <= timing && k.TimingRange.End >= timing)
                .ToHashSet();
            return categories;
        }

        void ExecuteCopy(int volume, int timing, HitsoundCache hitsoundCache, List<RawHitObject> unhandledObjectList,
            List<HitsoundCache> unhandledHitsoundFileList, List<StoryboardSampleData> samples, bool isCategoryScope)
        {
            if (isCategoryScope)
            {
                if (unhandledObjectList.Count == 0) return;
                GeneralCopy(volume, hitsoundCache, unhandledObjectList, unhandledHitsoundFileList);
            }
            else
            {
                if (unhandledObjectList.Count == 0)
                {
                    samples.Add(new StoryboardSampleData
                    {
                        Filename = hitsoundCache.SoundFile.GetRelativePath(OsuBeatmapDir),
                        Volume = (byte)volume,
                        Offset = timing
                    });
                    unhandledHitsoundFileList.Remove(hitsoundCache);
                }
                else
                {
                    GeneralCopy(volume, hitsoundCache, unhandledObjectList, unhandledHitsoundFileList);
                }
            }
        }

        void GeneralCopy(int volume, HitsoundCache hitsoundCache,
            List<RawHitObject> unhandledObjectList, List<HitsoundCache> unhandledHitsoundFileList)
        {
            var rawHitObject = unhandledObjectList[0];
            unhandledObjectList.RemoveAt(0);

            rawHitObject.FileName = hitsoundCache.SoundFile.GetRelativePath(OsuBeatmapDir);
            rawHitObject.SampleVolume = (byte)volume;
            unhandledHitsoundFileList.Remove(hitsoundCache);
        }
    }



    public static async Task<Project> LoadAsync(string projectPath)
    {
        var content = File.ReadAllText(projectPath);
        var projectVm = YamlConverter.DeserializeSettings<Project>(content);
        if (string.IsNullOrEmpty(projectVm.KsProjectVersion))
            throw new Exception("Invalid project file.");
        if (projectVm.KsProjectVersion != CurrentProjectVersion)
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
        project.KsProjectVersion = CurrentProjectVersion;

        if (!string.IsNullOrWhiteSpace(project.TemplateCsvFile))
        {
            project.LoadTemplateFile(project.TemplateCsvFile);
        }

        project.Engine = new AudioPlaybackEngine();

        var files = IOUtils.EnumerateFiles(project.OsuBeatmapDir, ".wav", ".ogg", ".osu");
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
                var hitsoundCache = await HitsoundCache.CreateAsync(project.Engine.WaveFormat,
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
                    soundCategoryVm.SoundFiles.Add(cache.SoundFile);
                }
                else
                {
                    soundCategoryVm.SoundFiles.Add(new SoundFile
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