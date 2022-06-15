using System;
using System.Globalization;
using System.Windows.Data;
using KSTool.Om.Core.Models;

namespace KSTool.Om.Converters;

internal class TitleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Project project)
        {
            return project.ProjectName + " - KSTool";
        }

        return "KSTool";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}