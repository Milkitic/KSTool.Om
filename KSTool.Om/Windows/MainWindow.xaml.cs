using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HandyControl.Controls;
using KSTool.Om.Core;
using KSTool.Om.Core.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using Milki.Extensions.MouseKeyHook;
using ModifierKeys = Milki.Extensions.MouseKeyHook.ModifierKeys;
using Window = System.Windows.Window;

namespace KSTool.Om.Windows;

public class MainWindowViewModel : ViewModelBase
{
    private Project? _project;
    private bool _isLoading;

    public Project? Project
    {
        get => _project;
        set => this.RaiseAndSetIfChanged(ref _project, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly List<Guid> _hotKeyHandles = new();
    private bool _opening;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel = new MainWindowViewModel();
        App.Current.KeyboardHook.RegisterHotkey(ModifierKeys.Control, HookKeys.O, async (_, _, type) =>
        {
            if (type == KeyAction.KeyDown)
            {
                await Dispatcher.InvokeAsync(async () => await OpenProjectAsync());
            }
        });
        AudioHelper.RegisterAudioPlaying(lbHitsounds);
    }

    private async void miCreateProject_OnClick(object sender, RoutedEventArgs e)
    {
        if (!PreCloseProject()) return;

        var window = new CreateProjectWindow
        {
            Owner = this
        };
        var result = window.ShowDialog();
        if (result == true)
        {
            if (!CloseProject()) return;
            LoadProject(await Project.CreateNewAsync(window.ProjectName!, window.BeatmapDirectory!));
        }
    }

    private void miCloseProject_OnClick(object sender, RoutedEventArgs e)
    {
        CloseProject();
    }

    private void miSaveProject_OnClick(object sender, RoutedEventArgs e)
    {
        SaveProject();
    }

    private async void miOpenProject_OnClick(object sender, RoutedEventArgs e)
    {
        await OpenProjectAsync();
    }

    private void LoadProject(Project project)
    {
        _viewModel.Project = project;
        RegisterHotKeys();
    }

    private void miOpenBeatmapFolder_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(_viewModel.Project!.OsuBeatmapDir)
        {
            UseShellExecute = true
        });
    }

    private void miExit_OnClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (!PreCloseProject())
        {
            e.Cancel = true;
        }

        CloseProject();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel.Project != null)
        {
            var currentDifficulty = _viewModel.Project!.CurrentDifficulty;
            if (currentDifficulty is { IsDifficultyLost: false, OsuFile: { } })
            {
                timelineViewer.Load(currentDifficulty.OsuFile);
            }
        }
    }

    private void SaveProject()
    {
        string? savePath = _viewModel.Project!.ProjectPath;
        if (savePath == null)
        {
            var ofd = new CommonSaveFileDialog
            {
                DefaultFileName = _viewModel.Project.ProjectName,
                DefaultExtension = "ksproj"
            };
            ofd.Filters.Add(new CommonFileDialogFilter("KS Project", "ksproj"));

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var folder = ofd.FileName;
                savePath = folder;
            }
            else
            {
                return;
            }
        }

        _viewModel.Project.Save(savePath);
        _viewModel.Project.ProjectPath = savePath;

        Growl.Success("Project Saved.");
    }

    private void RegisterHotKeys()
    {
        _hotKeyHandles.Add(App.Current.KeyboardHook.RegisterHotkey(
            ModifierKeys.Control, HookKeys.S, (_, _, type) =>
            {
                if (type == KeyAction.KeyDown)
                {
                    Dispatcher.Invoke(() =>
                    {
                        this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                        SaveProject();
                    });
                }
            }));
        _hotKeyHandles.Add(App.Current.KeyboardHook.RegisterHotkey(
            ModifierKeys.Control, HookKeys.Left, (_, _, type) =>
            {
                if (type == KeyAction.KeyDown)
                {
                    Dispatcher.Invoke(() => categoryManager.AddSoundToCategory());
                }
            }));
    }

    private void UnregisterHotKeys()
    {
        foreach (var hotKeyHandle in _hotKeyHandles)
        {
            App.Current.KeyboardHook.TryUnregister(hotKeyHandle);
        }

        _hotKeyHandles.Clear();
    }

    private async Task OpenProjectAsync()
    {
        if (_opening)
        {
            return;
        }

        try
        {
            _opening = true;
            if (!PreCloseProject()) return;

            var ofd = new CommonOpenFileDialog
            {
                DefaultExtension = "ksproj"
            };

            ofd.Filters.Add(new CommonFileDialogFilter("KS Project", "ksproj"));

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _opening = false;
                var fileName = ofd.FileName;
                _viewModel.IsLoading = true;
                try
                {
                    var project = await Project.LoadAsync(fileName);
                    if (!CloseProject()) return;
                    LoadProject(project);
                }
                catch (Exception ex)
                {
                    Growl.Error($"Error while loading project: \r\n{ex.Message}\r\n{fileName}");
                }
                finally
                {
                    _viewModel.IsLoading = false;
                }
            }
        }
        finally
        {
            _opening = false;
        }
    }

    private bool CloseProject()
    {
        if (!PreCloseProject()) return false;

        _viewModel.Project = null;
        // ...

        UnregisterHotKeys();
        return true;
    }

    private bool PreCloseProject()
    {
        if (_viewModel.Project?.IsModified == true)
        {
            //...
        }

        return true;
    }

    private void cbShowUsed_OnCheckChanged(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Project != null)
        {
            _viewModel.Project.ComputeUnusedHitsounds();
            _viewModel.Project.RefreshShowHitsoundType();
        }
    }

    private void btnAddRule_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Project?.CurrentDifficulty == null) return;
        var addWin = new AddOrEditRuleWindow(_viewModel.Project, _viewModel.Project.CurrentDifficulty)
        {
            Owner = this
        };
        addWin.ShowDialog();
    }

    private void btnDelRule_OnClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var timingRule = (TimingRule)button.Tag;
        if (_viewModel.Project?.CurrentDifficulty == null) return;

        _viewModel.Project.CurrentDifficulty.FlattenTimingRules.Remove(timingRule);
    }

    private void btnEditRule_OnClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var timingRule = (TimingRule)button.Tag;
        if (_viewModel.Project?.CurrentDifficulty == null) return;

        var addWin = new AddOrEditRuleWindow(_viewModel.Project, _viewModel.Project.CurrentDifficulty, timingRule)
        {
            Owner = this
        };
        addWin.ShowDialog();
    }

    private async void btnExport_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Project == null) return;
        try
        {
            if (_viewModel.Project.TemplateCsvFile == null)
                throw new Exception("You haven't selected any template files!");
            _viewModel.Project.LoadTemplateFile(_viewModel.Project.TemplateCsvFile);
        }
        catch (Exception ex)
        {
            Growl.Error("Load template error: " + ex.Message);
            return;
        }

        try
        {
            var ignoreSamples = _viewModel.Project.EditorSettings.IgnoreSamplesChecked;
            var path = await _viewModel.Project.ExportCurrentDifficultyAsync(ignoreSamples);
            Growl.Success("Export keysound to " + path);
        }
        catch (Exception ex)
        {
            Growl.Error("Export error: " + ex.Message);
        }
    }

    private void btnBrowseTemplate_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Project == null) return;
        var ofd = new CommonOpenFileDialog
        {
            DefaultExtension = "csv",
            Multiselect = false
        };

        ofd.Filters.Add(new CommonFileDialogFilter("CSV file", "csv"));

        if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
        {
            var file = ofd.FileName;
            _viewModel.Project.TemplateCsvFile = file;
        }
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var result = await Updater.CheckUpdateAsync();

        if (result == true)
        {
            Growl.Ask($"Found new version: {Updater.NewRelease!.NewVerString}. " +
                      $"Click yes to open the release page.",
                dialogResult =>
                {
                    if (dialogResult)
                    {
                        Updater.OpenLastReleasePage();
                    }

                    return dialogResult;
                });
        }
    }

    private void miOpenWebPage_OnClick(object sender, RoutedEventArgs e)
    {
        var frameworkElement = (FrameworkElement)sender;
        var tag = (string?)frameworkElement.Tag;

        if (tag != null)
        {
            Process.Start(new ProcessStartInfo(tag) { UseShellExecute = true });
        }
    }
}