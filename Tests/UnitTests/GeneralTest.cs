using System.Collections.ObjectModel;
using KSTool.Om.Core;
using KSTool.Om.Core.Models;

namespace UnitTests;

public class GeneralTest
{
    [Fact]
    public async Task TestProject()
    {
        Directory.CreateDirectory("Files");

        var createNew = await Project.CreateNewAsync("Test",
            @"C:\Users\milkitic\Desktop\1002455 supercell - Giniro Hikousen  (Ttm bootleg Edit)");

        var soundCategoryVm = new SoundCategory
        {
            Name = "Kicks",
        };

        createNew.AddSoundFileIntoSoundCategory(
            createNew.HitsoundFiles.First(k => k.Key == "soft-hitwhistle10.wav").Value,
            soundCategoryVm
        );

        soundCategoryVm.SoundFileNames.Add("NOTEXIST.wav");
        soundCategoryVm.Hitsounds.Add(HitsoundCache.CreateLost("NOTEXIST.wav"));
        createNew.TemplateCsvFile = "Files/template.csv";
        createNew.SoundCategories.Add(soundCategoryVm);

        var diff = createNew.Difficulties.First(k => k.DifficultyName == "4K Hard");
        diff.GroupTimingRules.Add(new GroupTimingRule
        {
            PreferredCategory = "Asdf",
            RangeInfos = new List<RangeInfo>
            {
                new() { TimingRange = new RangeValue<int>(1234, 5678) },
                new() { TimingRange = new RangeValue<int>(6000, 8000) }
            }
        });

        diff.GroupTimingRules.Add(new GroupTimingRule
        {
            PreferredCategory = "Kicks",
            RangeInfos = new List<RangeInfo>
            {
                new() { TimingRange = new RangeValue<int>(0, 23456) },
                new() { TimingRange = new RangeValue<int>(25000, 1130000) }
            }
        });

        createNew.Difficulties.Add(new ProjectDifficulty
        {
            DifficultyName = "NONEXISTDIFF"
        });

        createNew.Save("Files/createNew.ksproj");
        var load = await Project.LoadAsync("Files/createNew.ksproj");
        load.Save("Files/load.ksproj");
        load.CurrentDifficulty = load.Difficulties.FirstOrDefault(k => k.DifficultyName.Contains("4K Hard"));
        await load.ExportCurrentDifficultyAsync();
    }

    [Fact]
    public async Task TestConvertHelper()
    {
        await TemplateHelper.ConvertOsuFileToTemplate(
            @"E:\Games\osu!\Songs\BmsToOsu\IIDX\29075\",
            "lv.10", @"E:\Games\osu!\Songs\BmsToOsu\IIDX\29075\Project\template.csv");
    }
}