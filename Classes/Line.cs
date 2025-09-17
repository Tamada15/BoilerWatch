using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace WpfApp1
{
    public class Line : LineSeries<DateTimePoint>
    {
        public ObservableCollection<DateTimePoint> Points { get; set; } = new();
        public ObservableCollection<DateTimePoint> FilterPoints { get; set; } = new();

        public string FileName;
        public Line(string Name, SKColor color,string FileName)
        {
            this.FileName = FileName;
            this.Name = Name;
            Values = FilterPoints;
            LineSmoothness = 0;
            GeometrySize = 1;
            Fill = new SolidColorPaint(color.WithAlpha(10));
            Stroke = new SolidColorPaint(color) { StrokeThickness = 0 };
            GeometryStroke = new SolidColorPaint(color, 3);
        }
    }
}
