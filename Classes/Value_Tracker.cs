using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Value_Tracker
    {
    private double _currentValue;
    private readonly Action<double> _onValueChanged;
    private readonly double _tolerance;

    // Конструктор
    public Value_Tracker(Action<double> onValueChanged, double initialValue = 0, double tolerance = 0.001)
    {
        _onValueChanged = onValueChanged;
        _currentValue = initialValue;
        _tolerance = tolerance; // Погрешность для сравнения double
    }

    // Метод для обновления значения (вызывайте его при получении новых данных)
    public void UpdateValue(double newValue)
    {
        if (Math.Abs(newValue - _currentValue) > _tolerance)
        {
            _currentValue = newValue;
            _onValueChanged?.Invoke(newValue); // Вызываем колбэк при изменении
        }
    }
    }
}
