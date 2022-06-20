using System.Windows;
using System.Windows.Controls;
using KSTool.Om.Core.Models;

namespace KSTool.Om.UserControls;

/// <summary>
/// SoundLibraryManager.xaml 的交互逻辑
/// </summary>
public partial class SoundLibraryManager : UserControl
{
    private Project? _viewModel;

    public SoundLibraryManager()
    {
        InitializeComponent();
        AudioHelper.RegisterAudioPlaying(lbHitsounds);
    }

    private Project? Project => _viewModel ??= (Project?)DataContext;

    private void cbShowUsed_OnCheckChanged(object sender, RoutedEventArgs e)
    {
        if (Project == null) return;
        Project.ComputeUnusedHitsounds();
        Project.RefreshShowHitsoundType();
    }
}