using CsvHelper.Configuration.Attributes;

namespace KSTool.Om.Core.Models;

/// <summary>
///
/// <remarks>https://joshclose.github.io/CsvHelper/getting-started/</remarks>
/// </summary>
public class TemplateObject
{
    [Index(0)]
    public int Timing { get; set; }

    [Index(1)]
    public string RelativePath { get; set; }
}