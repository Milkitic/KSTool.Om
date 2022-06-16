using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KSTool.Om.Core.Models;

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
}