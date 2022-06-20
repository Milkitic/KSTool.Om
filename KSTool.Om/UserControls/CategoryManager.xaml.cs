using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KSTool.Om.Core.Models;
using KSTool.Om.Windows;

namespace KSTool.Om.UserControls;

/// <summary>
/// CategoryManager.xaml 的交互逻辑
/// </summary>
public partial class CategoryManager : UserControl
{
    private Project? _viewModel;

    public CategoryManager()
    {
        InitializeComponent();
        AudioHelper.RegisterAudioPlaying(lbCategoryHitsounds);
    }

    private Project Project => _viewModel ??= (Project)DataContext;

    private void btnAddCategory_OnClick(object sender, RoutedEventArgs e)
    {
        const string str = "Category ";

        var filters = Project.SoundCategories
            .Where(k => k.Name.StartsWith(str))
            .Select(k => k.Name)
            .ToHashSet();
        int index = 1;
        var categoryName = str + index;
        while (filters.Contains(categoryName))
        {
            index++;
            categoryName = str + index;
        }

        var soundCategory = new SoundCategory { Name = categoryName };
        Project.SoundCategories.Add(soundCategory);
        Project.SelectedCategory = soundCategory;
    }

    private void btnDelCategory_OnClick(object sender, RoutedEventArgs e)
    {
        if (Project.SelectedCategory != null)
        {
            var index = Project.SoundCategories.IndexOf(Project.SelectedCategory);
            Project.SoundCategories.Remove(Project.SelectedCategory);
            if (index == 0)
            {
                Project.SelectedCategory = Project.SoundCategories.FirstOrDefault();
            }
            else if (index >= Project.SoundCategories.Count)
            {
                Project.SelectedCategory = Project.SoundCategories.LastOrDefault();
            }
            else
            {
                Project.SelectedCategory = Project.SoundCategories[index];
            }
        }
    }

    private void tbCategoryName_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (Project.SelectedCategory != null)
        {
            Project.SelectedCategory.Name = tbCategoryName.Text;
        }
    }

    private void tbCategoryName_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (Project.SelectedCategory != null)
        {
            if (e.Key == Key.Enter)
            {
                Project.SelectedCategory.Name = tbCategoryName.Text;
                tbCategoryName.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            }
        }
    }

    private void btnAddCategorySound_OnClick(object sender, RoutedEventArgs e)
    {
        AddSoundToCategory();
    }

    internal void AddSoundToCategory()
    {
        /*SelectedHitsound: { } cache,*/
        if (Project is not { SelectedCategory: { } category }) return;
        var mainWindow = (MainWindow)App.Current.MainWindow!;
        foreach (var selectedItem in mainWindow.soundLibraryManager.lbHitsounds.SelectedItems)
        {
            if (selectedItem is HitsoundCache hitsoundCache)
            {
                category.Hitsounds.Add(hitsoundCache);
            }
        }

        if (!Project.EditorSettings.ShowUsedChecked)
        {
            Project.ComputeUnusedHitsounds();
            Project.RefreshShowHitsoundType();
        }
    }

    private void btnDelCategorySound_OnClick(object sender, RoutedEventArgs e)
    {
        if (Project is not { SelectedCategory: { SelectedSound: { } soundFile } category }) return;

        var selectedCategory = Project.SelectedCategory;
        var soundFiles = selectedCategory.Hitsounds;

        var index = soundFiles.IndexOf(soundFile);
        category.Hitsounds.Remove(soundFile);

        if (index == 0)
        {
            selectedCategory.SelectedSound = soundFiles.FirstOrDefault();
        }
        else if (index >= soundFiles.Count)
        {
            selectedCategory.SelectedSound = soundFiles.LastOrDefault();
        }
        else
        {
            selectedCategory.SelectedSound = soundFiles[index];
        }

        Reselect();
    }

    private void Reselect()
    {
        if (Project.EditorSettings.ShowUsedChecked) return;
        var mainWindow = (MainWindow)App.Current.MainWindow!;
        var index = mainWindow.soundLibraryManager.lbHitsounds.SelectedIndex;

        Project.ComputeUnusedHitsounds();
        Project.RefreshShowHitsoundType();

        if (index == 0)
        {
            var item = mainWindow.soundLibraryManager.lbHitsounds.Items.Cast<object>().FirstOrDefault();
            if (item != null) mainWindow.soundLibraryManager.lbHitsounds.ScrollIntoView(item);
        }
        else if (index >= Project.UnusedHitsoundFiles.Count)
        {
            var item = mainWindow.soundLibraryManager.lbHitsounds.Items.Cast<object>().LastOrDefault();
            if (item != null) mainWindow.soundLibraryManager.lbHitsounds.ScrollIntoView(item);
        }
        else
        {
            mainWindow.soundLibraryManager.lbHitsounds.ScrollIntoView(Project.UnusedHitsoundFiles[index]);
        }
    }
}