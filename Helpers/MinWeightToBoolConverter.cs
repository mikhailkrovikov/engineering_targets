/// <summary>
/// Конвертер для проверки наличия минимального порога веса (для UI)
/// </summary>
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace EngineeringTargets.Helpers
{
    public class MinWeightToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if (value is double minWeight)
            {
                return minWeight > 0;
            }

            if (double.TryParse(value.ToString(), NumberStyles.Float, culture, out double parsedValue))
            {
                return parsedValue > 0;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
