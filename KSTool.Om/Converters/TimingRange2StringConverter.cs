using System;
using System.Globalization;
using System.Windows.Data;

namespace KSTool.Om.Converters;

internal class TimingRange2StringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Core.Models.RangeValue<int> rangeValue)
        {
            return TimeSpan.FromMilliseconds(rangeValue.Start).ToString(@"mm\:ss\.fff") +
                   " ~ " + TimeSpan.FromMilliseconds(rangeValue.End).ToString(@"mm\:ss\.fff");
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}