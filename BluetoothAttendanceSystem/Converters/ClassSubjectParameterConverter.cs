using BluetoothAttendanceSystem.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothAttendanceSystem.Converters
{
    public class ClassSubjectParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values.Length == 2 && values[0] is string classIdString && Guid.TryParse(classIdString, out Guid classId)  && values[1] is int subjectId)
            {
                return new StudentSubjectModel { ClassID = classId, SubjectId = subjectId };
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
