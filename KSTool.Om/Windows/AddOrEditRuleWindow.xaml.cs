using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KSTool.Om.Core;
using KSTool.Om.Core.Models;

namespace KSTool.Om.Windows;

public class AddRuleWindowViewModel : ViewModelBase
{
    private SoundCategory? _selectedCategory;
    private int _startTime;
    private int _endTime;
    private int _volume;

    public AddRuleWindowViewModel(Project project, ProjectDifficulty projectDifficulty)
    {
        Project = project;
        ProjectDifficulty = projectDifficulty;
        SelectedCategory = project.SoundCategories.FirstOrDefault();
        Volume = -1;
    }

    public Project Project { get; }
    public ProjectDifficulty ProjectDifficulty { get; }

    public SoundCategory? SelectedCategory
    {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    public int StartTime
    {
        get => _startTime;
        set => this.RaiseAndSetIfChanged(ref _startTime, value);
    }

    public int EndTime
    {
        get => _endTime;
        set => this.RaiseAndSetIfChanged(ref _endTime, value);
    }

    public int Volume
    {
        get => _volume;
        set => this.RaiseAndSetIfChanged(ref _volume, value);
    }
}

/// <summary>
/// AddRuleWindow.xaml 的交互逻辑
/// </summary>
public partial class AddOrEditRuleWindow : Window
{
    private readonly TimingRule? _timingRule;
    private readonly AddRuleWindowViewModel _viewModel;
    private readonly Dictionary<TextBox, object?> _errorItems = new();

    public AddOrEditRuleWindow(Project project, ProjectDifficulty projectDifficulty, TimingRule? timingRule = null)
    {
        _timingRule = timingRule;
        InitializeComponent();
        DataContext = _viewModel = new AddRuleWindowViewModel(project, projectDifficulty);
        if (timingRule != null)
        {
            _viewModel.StartTime = timingRule.TimingRange.Start;
            _viewModel.EndTime = timingRule.TimingRange.End;
            _viewModel.Volume = timingRule.Volume ?? -1;
            _viewModel.SelectedCategory = timingRule.Category;

            Title = "Edit Rule Wizard";
            btnOk.Content = "Confirm";
            tbInstruction.Text = "Edit a Rule";
        }
    }

    private void btnOk_OnClick(object sender, RoutedEventArgs e)
    {
        if (_errorItems.Count > 0) return;

        var category = _viewModel.SelectedCategory;
        if (category == null) return;
        if (_viewModel.StartTime >= _viewModel.EndTime)
        {
            lblError.Content =
                $"The end time should be larger than start time.";
            lblError.Visibility = Visibility.Visible;
            return;
        }

        var rule = _viewModel.ProjectDifficulty.FlattenTimingRules
            .Where(k => k != _timingRule && k.Category == category)
            .FirstOrDefault(k => _viewModel.StartTime < k.TimingRange.End && _viewModel.EndTime > k.TimingRange.Start);
        if (rule != null)
        {
            lblError.Content =
                $"The time range is overlapped for current settings. ({rule.TimingRange.Start}~{rule.TimingRange.End})";
            lblError.Visibility = Visibility.Visible;
            return;
        }

        if (_timingRule == null)
        {
            _viewModel.ProjectDifficulty.FlattenTimingRules.Add(new TimingRule(category, new RangeInfo()
            {
                TimingRange = new RangeValue<int>(_viewModel.StartTime, _viewModel.EndTime),
                Volume = _viewModel.Volume == -1 ? null : _viewModel.Volume
            }));
        }
        else
        {
            _timingRule.Category = category;
            _timingRule.IsCategoryLost = false;
            _timingRule.TimingRange = new RangeValue<int>(_viewModel.StartTime, _viewModel.EndTime);
            _timingRule.Volume = _viewModel.Volume == -1 ? null : _viewModel.Volume;
        }

        DialogResult = true;
    }

    private void tbTime_OnLostFocus(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var input = textBox.Text;
        var split = input.Split(':', StringSplitOptions.RemoveEmptyEntries);
        TimeSpan ts = default;

        if (split.Length == 2)
        {
            input = $"00:{input}";
        }
        else if (split.Length == 1)
        {
            var split2 = input.Split('.');
            if (split2.Length == 1)
            {
                ts = TimeSpan.FromMilliseconds(long.Parse(input));
            }
        }

        if (ts != default || TimeSpan.TryParse(input, out ts))
        {
            var exp = textBox.GetBindingExpression(TextBox.TextProperty);
            if (exp != null)
            {
                var t = exp.DataItem.GetType();
                var prop = t.GetProperty(exp.ResolvedSourcePropertyName);
                var tsTotalMilliseconds = (int)ts.TotalMilliseconds;
                if (tsTotalMilliseconds < 0) tsTotalMilliseconds *= -1;
                prop?.SetValue(exp.DataItem, tsTotalMilliseconds);
            }

            _errorItems.Remove(textBox);
            if (_errorItems.Count > 0)
            {
                lblError.Content = _errorItems.First().Value;
                lblError.Visibility = Visibility.Visible;
            }
            else
            {
                lblError.Visibility = Visibility.Collapsed;
            }

            return;
        }

        lblError.Content = "Should be a valid TimeSpan format. e.g. 01:08.123";
        lblError.Visibility = Visibility.Visible;
        _errorItems.TryAdd(textBox, lblError.Content);
    }
}