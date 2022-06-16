using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace KSTool.Om.Converters;

public class Int2ArrayConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<int, object[]> SharedDictionary = new();
    private static readonly object SharedObject = new();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i)
            return SharedDictionary.GetOrAdd(i, j => Enumerable.Repeat(SharedObject, j).ToArray());
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}