using System.Globalization;
using Coosu.Beatmap;
using Coosu.Beatmap.Extensions.Playback;
using CsvHelper;
using KSTool.Om.Core.Models;

namespace KSTool.Om.Core;

public static class TemplateHelper
{
    public static async Task ConvertOsuFileToTemplate(string beatmapDir, string difficultyName, string outputPath)
    {
        var dir = new OsuDirectory(beatmapDir);
        await dir.InitializeAsync();
        var osuFile = dir.OsuFiles.First(k => k.Metadata.Version == difficultyName);
        var ok = await dir.GetHitsoundNodesAsync(osuFile);

        var sb = ok.Where(k => !k.UseUserSkin && k is PlayableNode).Cast<PlayableNode>().ToList();
        var samples = osuFile.Events.Samples;
        var records = new List<TemplateObject>();
        foreach (var storyboardSampleData in samples)
        {
            records.Add(new TemplateObject
            {
                RelativePath = storyboardSampleData.Filename,
                Timing = storyboardSampleData.Offset
            });
        }

        foreach (var playableNode in sb)
        {
            records.Add(new TemplateObject
            {
                RelativePath = playableNode.Filename!,
                Timing = playableNode.Offset
            });
        }

        records.Sort((a, b) => a.Timing.CompareTo(b.Timing));
        await using var writer = new StreamWriter(outputPath);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        foreach (var record in records)
        {
            csv.WriteRecord(record);
            await csv.NextRecordAsync();
        }
    }
}