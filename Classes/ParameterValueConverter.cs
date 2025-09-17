using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WpfApp1;

namespace WpfApp1
{
    public class ParameterValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<Parameters> parametersCollection && parameter is string targetId)
            {
                // Ищем элемент по ID
                var param = parametersCollection.FirstOrDefault(p => p.Id == targetId);


                if (param != null)
                {
                    // Пытаемся преобразовать значение в число и форматировать
                    if (double.TryParse(param.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double numericValue))
                    {
                        // Если это число, форматируем до одного знака после запятой
                        return numericValue.ToString("F1", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        // Если это не число, возвращаем исходное значение (строку)
                        return param.Value;
                    }
                }
            }
            return null; // Или throw new ArgumentException("...");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Этот метод не используется для односторонней привязки, но должен быть реализован
            throw new NotImplementedException();
        }
    }
}
