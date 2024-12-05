using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothAttendanceSystem.Converters
{
    public class GenderBackgroundColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is string gender)
            {
                return gender.ToLower() == "female"
                    ? Color.FromArgb("#FFB6C1")
                    : Color.FromArgb("3b82f6");
            }
            return Color.FromArgb("#FFFFFF");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
