using System;
using System.Globalization;
using System.Windows.Data;
using KSTool.Om.Objects;

namespace KSTool.Om.Converters;

public class HeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (/*values.Length == 3 &&*/
            values[0] is ObjectBase ob
           //&&
           //values[1] is double startX &&
           //values[2] is double endX
           )
        {
            if (ob is Hold hold)
            {
                var len = (hold.RealtimeEndY - hold.RealtimeY) + 15d;
                //Console.WriteLine(len);
                return len;
            }
            else
            {
                return 15d;
            }
        }

        return 15d;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}