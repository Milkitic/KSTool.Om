using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace KSTool.Om.UserControls;

public class CachingItemsControl : ItemsControl
{
    private readonly Stack<DependencyObject> itemContainers =
        new Stack<DependencyObject>();

    protected override DependencyObject GetContainerForItemOverride()
    {
        return itemContainers.Count > 0
            ? itemContainers.Pop()
            : base.GetContainerForItemOverride();
    }

    protected override void ClearContainerForItemOverride(
        DependencyObject element, object item)
    {
        itemContainers.Push(element);
    }
}