using System.Globalization;
using System.Windows.Data;

namespace SolyankaGuide
{
    public class GriddedRatioConverter : IValueConverter
    {
        private const double Ratio = 11.0 / 16.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
                return width * Ratio;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}