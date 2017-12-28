using System;
using System.Windows.Data;
using System.Windows.Media;

namespace archilabUI.TextNotePlus
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && (bool)value)
            {
                return (bool)value ? Brushes.White : (SolidColorBrush)new BrushConverter().ConvertFromString("#e8e6e3");
            }
            return (SolidColorBrush)new BrushConverter().ConvertFromString("#e8e6e3");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
