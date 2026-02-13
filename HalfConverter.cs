using System.Globalization;
using System.Windows.Data;

namespace SolyankaGuide
{
    public class HalfConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalWidth)
            {
                return (totalWidth / 2);
            }
            return 100.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}