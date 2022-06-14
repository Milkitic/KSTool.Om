using KSTool.Om.Core.Models;

namespace UnitTests;

public class GeneralTest
{
    [Fact]
    public async Task TestProject()
    {
        var createNew = await Project.CreateNewAsync("Test",
            @"C:\Users\milkitic\Desktop\1002455 supercell - Giniro Hikousen  (Ttm bootleg Edit)");

        var soundCategoryVm = new SoundCategory()
        {
            Name = "Kicks",
        };

        createNew.AddSoundFileIntoSoundCategory(
            createNew.HitsoundFiles.First(k => k.Key == "soft-hitwhistle9.wav").Value.SoundFile,
            soundCategoryVm);

        soundCategoryVm.SoundFiles.Add("NOTEXIST.wav");
        soundCategoryVm.SoundFileVms.Add(new SoundFile()
        {
            FilePath = "NOTEXIST.wav",
            IsFileLost = true
        });
        createNew.TemplateCsvFile = "template.csv";
        createNew.SoundCategories.Add(soundCategoryVm);
        createNew.Save("createNew.ksproj");

        var load = await Project.LoadAsync("createNew.ksproj");
        load.Save("load.ksproj");
    }
}