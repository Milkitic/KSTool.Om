using System;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using KSTool.Om.Core.Models;
using KSTool.Om.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace KSTool.Om.UserControls;

/// <summary>
/// DifficultyManager.xaml 的交互逻辑
/// </summary>
public partial class DifficultyManager : UserControl
{
    private Project? _viewModel;

    public DifficultyManager()
    {
        InitializeComponent();
    }

    private Project? Project => _viewModel ??= (Project?)DataContext;
    private MainWindow MainWindow => (MainWindow)App.Current.MainWindow!;

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Project == null) return;
        var currentDifficulty = Project!.CurrentDifficulty;
        if (currentDifficulty is { IsDifficultyLost: false, OsuFile: { } })
        {
            MainWindow.timelineViewer.Load(currentDifficulty.OsuFile);
        }
    }

    private void btnBrowseTemplate_OnClick(object sender, RoutedEventArgs e)
    {
        if (Project == null) return;
        var ofd = new CommonOpenFileDialog
        {
            DefaultExtension = "csv",
            Multiselect = false
        };

        ofd.Filters.Add(new CommonFileDialogFilter("CSV file", "csv"));

        if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
        {
            var file = ofd.FileName;
            Project.TemplateCsvFile = file;
        }
    }

    private void btnAddRule_OnClick(object sender, RoutedEventArgs e)
    {
        if (Project?.CurrentDifficulty == null) return;
        var addWin = new AddOrEditRuleWindow(Project, Project.CurrentDifficulty)
        {
            Owner = MainWindow
        };
        addWin.ShowDialog();
    }

    private void btnDelRule_OnClick(object sender, RoutedEventArgs e)
    {
        if (Project?.CurrentDifficulty == null) return;
        var button = (Button)sender;
        var timingRule = (TimingRule)button.Tag;

        Project.CurrentDifficulty.FlattenTimingRules.Remove(timingRule);
    }

    private void btnEditRule_OnClick(object sender, RoutedEventArgs e)
    {
        if (Project?.CurrentDifficulty == null) return;
        var button = (Button)sender;
        var timingRule = (TimingRule)button.Tag;

        var addWin = new AddOrEditRuleWindow(Project, Project.CurrentDifficulty, timingRule)
        {
            Owner = MainWindow
        };
        addWin.ShowDialog();
    }

    private async void btnExport_OnClick(object sender, RoutedEventArgs e)
    {
        if (Project == null) return;
        try
        {
            if (Project.TemplateCsvFile == null)
                throw new Exception("You haven't selected any template files!");
            Project.LoadTemplateFile(Project.TemplateCsvFile);
        }
        catch (Exception ex)
        {
            Growl.Error("Load template error: " + ex.Message);
            return;
        }

        try
        {
            var ignoreSamples = Project.EditorSettings.IgnoreSamplesChecked;
            var path = await Project.ExportCurrentDifficultyAsync(ignoreSamples);
            Growl.Success("Export keysound to " + path);
        }
        catch (Exception ex)
        {
            Growl.Error("Export error: " + ex.Message);
        }
    }
}