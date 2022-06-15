using System;
using System.Globalization;
using System.Windows.Data;

namespace KSTool.Om.Converters;

internal class IsNullToIsEnabledConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}