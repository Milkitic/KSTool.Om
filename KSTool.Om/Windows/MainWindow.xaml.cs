using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using KSTool.Om.Core;
using KSTool.Om.Core.Models;
using Microsoft.WindowsAPICodePack.Dialogs;

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

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel = new MainWindowViewModel();
    }

    private bool CloseProject()
    {
        if (!PreCloseProject()) return false;

        _viewModel.Project?.Dispose();
        _viewModel.Project = null;
        // ...
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
            _viewModel.Project = await Project.CreateNewAsync(window.ProjectName!, window.BeatmapDirectory!);
        }
    }

    private void miSaveProject_OnClick(object sender, RoutedEventArgs e)
    {
        string? savePath = _viewModel.Project!.ProjectPath;
        if (savePath == null)
        {
            var ofd = new CommonSaveFileDialog
            {
                DefaultFileName = _viewModel.Project.ProjectName,
                DefaultExtension = ".ksproj"
            };
            ofd.Filters.Add(new CommonFileDialogFilter("KS Project", ".ksproj"));

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
    }

    private async void miOpenProject_OnClick(object sender, RoutedEventArgs e)
    {
        if (!PreCloseProject()) return;

        var ofd = new CommonOpenFileDialog
        {
            DefaultExtension = ".ksproj"
        };
        ofd.Filters.Add(new CommonFileDialogFilter("KS Project", ".ksproj"));

        if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
        {
            var fileName = ofd.FileName;
            _viewModel.IsLoading = true;
            try
            {
                if (!CloseProject()) return;
                _viewModel.Project = await Project.LoadAsync(fileName);
            }
            catch (Exception ex)
            {
                MsgDialog.Error("Please choose another one.", ex.Message);
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }
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
}