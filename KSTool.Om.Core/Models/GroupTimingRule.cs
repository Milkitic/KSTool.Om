namespace KSTool.Om.Core.Models;

public class GroupTimingRule
{
    #region Configurable

    public string PreferredGroupName { get; set; } = "New Group";
    public List<RangeValue<int>> TimingRanges { get; set; } = new();

    #endregion
}