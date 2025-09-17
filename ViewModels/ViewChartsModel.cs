using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.VisualElements;
using SkiaSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WpfApp1
{
    public partial class ViewChartsModel : ObservableObject
    {
        private static readonly double ZoomFactor = 3.5;
        private static readonly long MinRangeTicks = TimeSpan.FromMinutes(10).Ticks;
        private static readonly long MinZoomRangeTicks = TimeSpan.FromMinutes(59).Ticks; // Минимальный диапазон 5 минут
        private static readonly long[] ZoomLevelThresholds = new long[]
    {
        TimeSpan.FromHours(12).Ticks,  // Уровень 1
        TimeSpan.FromHours(6).Ticks,   // Уровень 2  
        TimeSpan.FromHours(3).Ticks,   // Уровень 3
        TimeSpan.FromHours(1).Ticks    // Уровень 4 (максимальный)
    };

        public List<Line> Lines { get; set; }
        public List<ParameterValue> point_1;
        public List<ParameterValue> point_2;
        public List<ParameterValue> point_3;
        public List<ParameterValue> point_4;

        [ObservableProperty]
        private DateTime startDate;
        private DateTime CurrentDate;
        public Axis[] XAxes { get; }
        public Axis[] Yxes { get; }
        public ISeries[] Series { get; set; }
        public SolidColorPaint LegendTextPaint { get; set; }
        public ViewChartsModel(string name_1,
            SKColor color_1,
            string name_2,
            SKColor color_2,
            string name_3,
            SKColor color_3,
            string name_4,
            SKColor color_4,
            string Charts)
        {
            point_1 = new List<ParameterValue>();
            point_2 = new List<ParameterValue>();
            point_3 = new List<ParameterValue>();
            point_4 = new List<ParameterValue>();
            Lines = new List<Line>
            {
                new Line(name_1, color_1,Charts+"_DataLine1"),
                new Line(name_2, color_2,Charts+"_DataLine2"),
                new Line(name_3, color_3,Charts+"_DataLine3"),
                new Line(name_4, color_4,Charts+"_DataLine4")
            };

            Series = new ISeries[]
            {
                Lines[0],
                Lines[1],
                Lines[2],
                Lines[3],
            };

            StartDate = DateTime.Now.Date;
            CurrentDate = StartDate;
            XAxes = new Axis[]
            {
              new Axis
              {
              Labeler = value =>
               {
                    var date = new DateTime((long)value);
                       return $"{date:dd.MM HH:mm}";
               },
              AnimationsSpeed = TimeSpan.FromMilliseconds(0),
              Name = "Время", // Подпись оси
              NameTextSize = 12,

              TextSize = 10,
              UnitWidth = TimeSpan.FromHours(12).Ticks,
              MinStep = TimeSpan.FromMinutes(10).Ticks,
              MinLimit = StartDate.Ticks,
              MaxLimit = StartDate.AddDays(1).Ticks,

              }
            };

            Yxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 200
                }
            };

            LegendTextPaint =
                new SolidColorPaint
                {
                    Color = SKColors.White,
                    FontFamily = "Roboto",
                    SKTypeface = SKTypeface.FromFamilyName("Bolt"),
                };
        }



        public async Task AddPoint(Line line, List<ParameterValue> points)
        {
            line.Points.Clear();
            line.Points = ConvertToDateTimePoints(points);
            await ApplyFilter(CurrentDate);

        }

        public async Task AddAllPoint()
        {
            await Task.WhenAll(

                AddPoint(Lines[0], point_1),
                AddPoint(Lines[1], point_2),
                AddPoint(Lines[2], point_3),
                AddPoint(Lines[3], point_4)
            );
        }

     




        [RelayCommand]
        public async Task ApplyFilter()
        {
            CurrentDate = StartDate.Date;
            var nextDay = CurrentDate.AddDays(1);

            var (filtered_1, filtered_2, filtered_3, filtered_4) = await Task.Run(() =>
            {
                // Используем LINQ-запросы с предварительной материализацией
                var filtered_1 = Lines[0].Points
                    .Where(p => p.DateTime >= CurrentDate && p.DateTime < nextDay);
                var filtered_2 = Lines[1].Points
                    .Where(p => p.DateTime >= CurrentDate && p.DateTime < nextDay);
                var filtered_3 = Lines[2].Points
                    .Where(p => p.DateTime >= CurrentDate && p.DateTime < nextDay);
                var filtered_4 = Lines[3].Points
                    .Where(p => p.DateTime >= CurrentDate && p.DateTime < nextDay);

                // Для больших коллекций используем ToArray() вместо ToList()
                return (
                    filtered_1.ToArray(),
                    filtered_2.ToArray(),
                    filtered_3.ToArray(),
                    filtered_4.ToArray()
                );
            });

            Lines[0].FilterPoints = new ObservableCollection<DateTimePoint>(filtered_1);
            Lines[1].FilterPoints = new ObservableCollection<DateTimePoint>(filtered_2);
            Lines[2].FilterPoints = new ObservableCollection<DateTimePoint>(filtered_3);
            Lines[3].FilterPoints = new ObservableCollection<DateTimePoint>(filtered_4);

            Series[0].Values = Lines[0].FilterPoints;
            Series[1].Values = Lines[1].FilterPoints;
            Series[2].Values = Lines[2].FilterPoints;
            Series[3].Values = Lines[3].FilterPoints;

            XAxes[0].MinLimit = CurrentDate.Ticks;
            XAxes[0].MaxLimit = nextDay.Date.Ticks;
        }

        public async Task ApplyFilter(DateTime data)
        {
            CurrentDate = data.Date;
            var nextDay = CurrentDate.AddDays(1);

            // Создаем копии коллекций ДО запуска Task.Run
            var pointsCopy1 = Lines[0].Points.ToArray();
            var pointsCopy2 = Lines[1].Points.ToArray();
            var pointsCopy3 = Lines[2].Points.ToArray();
            var pointsCopy4 = Lines[3].Points.ToArray();

            var (filtered_1, filtered_2, filtered_3, filtered_4) = await Task.Run(() =>
            {
                // Работаем с копиями, а не с оригинальными коллекциями
                var filtered_1 = pointsCopy1
                    .Where(p => p.DateTime >= CurrentDate && p.DateTime < nextDay)
                    .ToArray();

                var filtered_2 = pointsCopy2
                    .Where(p => p.DateTime >= CurrentDate && p.DateTime < nextDay)
                    .ToArray();

                var filtered_3 = pointsCopy3
                    .Where(p => p.DateTime >= CurrentDate && p.DateTime < nextDay)
                    .ToArray();

                var filtered_4 = pointsCopy4
                    .Where(p => p.DateTime >= CurrentDate && p.DateTime < nextDay)
                    .ToArray();

                return (filtered_1, filtered_2, filtered_3, filtered_4);
            });

            // Обновляем UI в основном потоке
            Lines[0].FilterPoints = new ObservableCollection<DateTimePoint>(filtered_1);
            Lines[1].FilterPoints = new ObservableCollection<DateTimePoint>(filtered_2);
            Lines[2].FilterPoints = new ObservableCollection<DateTimePoint>(filtered_3);
            Lines[3].FilterPoints = new ObservableCollection<DateTimePoint>(filtered_4);

            Series[0].Values = Lines[0].FilterPoints;
            Series[1].Values = Lines[1].FilterPoints;
            Series[2].Values = Lines[2].FilterPoints;
            Series[3].Values = Lines[3].FilterPoints;
        }
        private DateTime GetLastDataDate()
        {
            // Если обе коллекции пусты, возвращаем сегодняшнюю дату
            if (!Lines[0].Points.Any() && !Lines[1].Points.Any())
                return DateTime.Today;

            // Получаем последние даты из каждой коллекции
            DateTime lastTempDate = Lines[0].Points.Last().DateTime.AddMinutes(-5);
            DateTime lastDavDate = Lines[1].Points.Last().DateTime.AddMinutes(-5);

            // Возвращаем самую свежую дату
            return lastTempDate > lastDavDate ? lastTempDate : lastDavDate;
        }

        public async Task UpdateChartsPointsAsync(
                   List<ParameterValue> newPoints1,
                   List<ParameterValue> newPoints2,
                   List<ParameterValue> newPoints3,
                   List<ParameterValue> newPoints4)
        {
            await Task.Run(() =>
            {
                point_1.Clear();
                point_1.AddRange(newPoints1 ?? new List<ParameterValue>());

                point_2.Clear();
                point_2.AddRange(newPoints2 ?? new List<ParameterValue>());

                point_3.Clear();
                point_3.AddRange(newPoints3 ?? new List<ParameterValue>());

                point_4.Clear();
                point_4.AddRange(newPoints4 ?? new List<ParameterValue>());
            });

        }

        [RelayCommand]
        private void ZoomIn()
        {
            var axis = XAxes[0];
            if (axis.MinLimit == null || axis.MaxLimit == null) return;

            long currentMin = (long)axis.MinLimit;
            long currentMax = (long)axis.MaxLimit;

            if (currentMin >= currentMax) return;

            long currentRange = currentMax - currentMin;
            long newRange = (long)(currentRange / ZoomFactor);

            // Проверяем минимальный диапазон
            if (newRange < MinRangeTicks)
                newRange = MinRangeTicks;


            // Вычисляем центр текущего видимого диапазона
            long center = (currentMin + currentMax) / 2;

            // Вычисляем новые границы
            long newMin = center - newRange / 2;
            long newMax = center + newRange / 2;

            // Проверяем, чтобы не выйти за границы дня
            long dayStart = CurrentDate.Ticks;
            long dayEnd = CurrentDate.AddDays(1).Ticks;



            if (newMin < dayStart)
            {
                newMin = dayStart;
                newMax = newMin + newRange;
                if (newMax > dayEnd) newMax = dayEnd;
            }
            else if (newMax > dayEnd)
            {
                newMax = dayEnd;
                newMin = newMax - newRange;
                if (newMin < dayStart) newMin = dayStart;
            }

            long Range = newMax - newMin;

            if (Range < MinZoomRangeTicks)
                return;

            // Применяем новые границы
            axis.MinLimit = newMin;
            axis.MaxLimit = newMax;
        }

        [RelayCommand]
        private void ZoomOut()
        {
            var axis = XAxes[0];
            if (axis.MinLimit == null || axis.MaxLimit == null) return;

            long currentMin = (long)axis.MinLimit;
            long currentMax = (long)axis.MaxLimit;

            if (currentMin >= currentMax) return;

            long currentRange = currentMax - currentMin;
            long newRange = (long)(currentRange * ZoomFactor);


            // Вычисляем центр текущего видимого диапазона
            long center = (currentMin + currentMax) / 2;

            // Вычисляем новые границы
            long newMin = center - newRange / 2;
            long newMax = center + newRange / 2;

            // Проверяем, чтобы не выйти за границы дня
            long dayStart = CurrentDate.Ticks;
            long dayEnd = CurrentDate.AddDays(1).Ticks;

            if (newMin < dayStart)
            {
                newMin = dayStart;
                newMax = newMin + newRange;
                if (newMax > dayEnd) newMax = dayEnd;
            }
            else if (newMax > dayEnd)
            {
                newMax = dayEnd;
                newMin = newMax - newRange;
                if (newMin < dayStart) newMin = dayStart;
            }

            // Применяем новые границы
            axis.MinLimit = newMin;
            axis.MaxLimit = newMax;
        }

       


       

        public ObservableCollection<DateTimePoint> ConvertToDateTimePoints(List<ParameterValue> parameterValues)
        {
            var safeCopy = parameterValues.ToList();

            var localTimeZone = TimeZoneInfo.Local;

            return new ObservableCollection<DateTimePoint>(
                safeCopy
                    .Where(v => v != null) // Проверка на null объекта
                    .Where(v => !string.IsNullOrEmpty(v.Value)) // Проверка на null/empty Value
                    .Where(v => double.TryParse(v.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    .Select(v =>
                    {
                        var utcDateTime = DateTimeOffset.FromUnixTimeSeconds(v.Timestamp).DateTime;
                        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, localTimeZone);

                        return new DateTimePoint(
                            localDateTime,
                            Convert.ToDouble(v.Value, CultureInfo.InvariantCulture)
                        );
                    })
            );
        }
    }
}
