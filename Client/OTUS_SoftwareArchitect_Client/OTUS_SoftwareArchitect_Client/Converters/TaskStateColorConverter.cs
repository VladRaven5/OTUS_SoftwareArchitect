using OTUS_SoftwareArchitect_Client.Models;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Converters
{
    class TaskStateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is TaskState state)
            {
                switch (state)
                {
                    case TaskState.Closed:
                        return Color.Black;

                    case TaskState.Resolved:
                        return Color.Green;

                    case TaskState.Active:
                        return Color.Blue;

                    case TaskState.Proposed:
                        return Color.Gray;
                }
            }

            return Color.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
