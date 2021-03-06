using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HandyControl.Controls;
using KSTool.Om.Core;
using KSTool.Om.Core.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using Milki.Extensions.MouseKeyHook;
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
    private readonly List<Guid> _hotKeyGuids = new();
    private bool _opening;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel = new MainWindowViewModel();
        App.Current.KeyboardHook.RegisterHotkey(HookModifierKeys.Control, HookKeys.O, async (_, _, _) =>
        {
            await Dispatcher.InvokeAsync(async () => await OpenProjectAsync());
        });
    }

    private void LoadProject(Project project)
    {
        _viewModel.Project = project;
        RegisterHotKeys();
    }

    private void SaveProject()
    {
        if (_viewModel.Project!.ProjectPath == null)
        {
            SaveAsProject();
            return;
        }

        _viewModel.Project.Save(_viewModel.Project!.ProjectPath);
        Growl.Success("Project Saved.");
    }

    private bool SaveAsProject()
    {
        var ofd = new CommonSaveFileDialog
        {
            DefaultFileName = _viewModel.Project!.ProjectName,
            DefaultExtension = "ksproj"
        };
        ofd.Filters.Add(new CommonFileDialogFilter("KS Project", "ksproj"));

        if (ofd.ShowDialog() != CommonFileDialogResult.Ok) return false;
        _viewModel.Project.ProjectPath = ofd.FileName;
        _viewModel.Project.Save(ofd.FileName);
        Growl.Success("Project Saved.");
        return true;
    }

    private void RegisterHotKeys()
    {
        _hotKeyGuids.Add(App.Current.KeyboardHook.RegisterHotkey(HookModifierKeys.Control, HookKeys.S, (_, _, _) =>
        {
            Dispatcher.Invoke(() =>
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                SaveProject();
            });
        }));
        _hotKeyGuids.Add(App.Current.KeyboardHook.RegisterHotkey(HookModifierKeys.Control, HookKeys.Left, (_, _, _) =>
        {
            Dispatcher.Invoke(() => categoryManager.AddSoundToCategory());
        }));
    }

    private void UnregisterHotKeys()
    {
        foreach (var hotKeyHandle in _hotKeyGuids)
        {
            App.Current.KeyboardHook.TryUnregister(hotKeyHandle);
        }

        _hotKeyGuids.Clear();
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
                DefaultExtension = "ksproj",
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

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var result = await Updater.CheckUpdateAsync();

        if (result != true) return;
        var verString = Updater.NewRelease!.NewVerString;
        Growl.Ask($"Found new version: {verString}. Click yes to open the release page.", dialogResult =>
        {
            if (dialogResult)
            {
                Updater.OpenLastReleasePage();
            }

            return dialogResult;
        });
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (!PreCloseProject())
        {
            e.Cancel = true;
        }

        CloseProject();
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    private async void miCreateProject_OnClick(object sender, RoutedEventArgs e)
    {
        if (!PreCloseProject()) return;

        var window = new CreateProjectWindow
        {
            Owner = this
        };
        var result = window.ShowDialog();
        if (result != true) return;
        if (!CloseProject()) return;
        LoadProject(await Project.CreateNewAsync(window.ProjectName!, window.BeatmapDirectory!));
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

    private void miOpenBeatmapFolder_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(_viewModel.Project!.OsuBeatmapDir)
        {
            UseShellExecute = true
        });
    }

    private void miOpenProjectFolder_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Project == null) return;
        if (_viewModel.Project.ProjectPath == null)
        {
            if (!SaveAsProject())
            {
                return;
            }
        }

        var projectPath = _viewModel.Project.ProjectPath!;
        projectPath = Path.IsPathRooted(projectPath)
            ? projectPath
            : Path.Combine(Environment.CurrentDirectory, projectPath);

        Process.Start(new ProcessStartInfo("explorer.exe", string.Format("/select,\"{0}\"", projectPath))
        {
            UseShellExecute = true
        });
    }

    private void miExit_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
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