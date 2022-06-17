using System.Windows;
using System.Windows.Controls;
using KSTool.Om.Core;
using KSTool.Om.Core.Models;

namespace KSTool.Om;

// ReSharper disable once InconsistentNaming
public static class AudioHelper
{
    public static void RegisterAudioPlaying(ListBox listBox)
    {
        listBox.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
        listBox.SelectionChanged += ListBox_SelectionChanged;
    }

    private static void ListBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject dependencyObject) return;
        var container = ItemsControl.ContainerFromElement(sender as ListBox, dependencyObject);
        if (container is not ListBoxItem { DataContext: HitsoundCache hitsoundCache }) return;

        PlaySelected(hitsoundCache);
    }

    private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not { Count: > 0 }) return;
        if (e.AddedItems[0] is not HitsoundCache hitsoundCache) return;

        PlaySelected(hitsoundCache);
    }

    private static void PlaySelected(HitsoundCache hitsoundCache)
    {
        if (hitsoundCache.CachedSound == null) return;

        AudioManager.Instance.TryPlaySound(hitsoundCache.CachedSound);
    }
}