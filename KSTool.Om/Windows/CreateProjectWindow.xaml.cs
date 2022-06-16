using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace KSTool.Om.Windows;

/// <summary>
/// CreateProjectWindow.xaml 的交互逻辑
/// </summary>
public partial class CreateProjectWindow : Window
{
    public string? ProjectName { get; private set; }
    public string? BeatmapDirectory { get; private set; }

    public CreateProjectWindow()
    {
        InitializeComponent();
    }

    private void btnBrowse_OnClick(object sender, RoutedEventArgs e)
    {
        var ofd = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            Multiselect = false
        };

        if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
        {
            var folder = ofd.FileName;
            tbBeatmapDir.Text = folder;
        }
    }

    private void btnCreate_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbProjectName.Text))
        {
            lblError.Visibility = Visibility.Visible;
            lblError.Content = "Project name cannot be empty.";
            return;
        }

        if (string.IsNullOrWhiteSpace(tbBeatmapDir.Text))
        {
            lblError.Visibility = Visibility.Visible;
            lblError.Content = "Beatmap folder cannot be empty.";
            return;
        }

        if (!Directory.Exists(tbBeatmapDir.Text))
        {
            lblError.Visibility = Visibility.Visible;
            lblError.Content = "Beatmap folder is not exist.";
            return;
        }

        ProjectName = tbProjectName.Text;
        BeatmapDirectory = tbBeatmapDir.Text;
        DialogResult = true;
    }
}