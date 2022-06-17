using System;
using System.Globalization;
using System.Windows.Data;

namespace KSTool.Om.Converters;

internal class Volume2StringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null) return "Inherited";
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}