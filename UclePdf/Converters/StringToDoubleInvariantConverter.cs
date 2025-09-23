using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace UclePdf.Converters
{
    public class StringToDoubleInvariantConverter : IValueConverter
    {
        private static readonly Regex ValidPattern = new Regex(@"^\d*([\.,]\d{0,2})?$", RegexOptions.Compiled);

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                var r = Math.Round(d, 2, MidpointRounding.AwayFromZero);
                return r.ToString("0.##", CultureInfo.InvariantCulture);
            }
            if (value is double?)
            {
                var nd = (double?)value;
                if (nd.HasValue)
                {
                    var r = Math.Round(nd.Value, 2, MidpointRounding.AwayFromZero);
                    return r.ToString("0.##", CultureInfo.InvariantCulture);
                }
                return string.Empty;
            }
            return string.Empty;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                s = s.Trim();
                if (string.IsNullOrEmpty(s)) return null;

                // Permitir solo números con '.' o ',' y hasta 2 decimales (no confirmar si termina en separador)
                if (!ValidPattern.IsMatch(s) || s.EndsWith(".") || s.EndsWith(","))
                    return Binding.DoNothing;

                s = s.Replace(',', '.');
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                {
                    return Math.Round(d, 2, MidpointRounding.AwayFromZero);
                }
                return Binding.DoNothing;
            }
            return Binding.DoNothing;
        }
    }
}
