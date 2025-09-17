using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WpfApp1
{
    public partial class MainViewModel : ObservableObject
    {

        public ViewChartsModel viewCharts { get; set; }
        public ViewChartsModel viewCharts_2 { get; set; }
        public AppStateViewModel appState { get; set; }

        public HTTP_Client hTTP { get; set; }

        private DispatcherTimer _timer;

        public DateTime StartDate
        {
            get => viewCharts.StartDate;
            set
            {
                if (viewCharts.StartDate != value)
                {
                    viewCharts.StartDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime StartDate_2
        {
            get => viewCharts_2.StartDate;
            set
            {
                if (viewCharts_2.StartDate != value)
                {
                    viewCharts_2.StartDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel(AppStateViewModel app)
        {
            appState = app;
            viewCharts = new ViewChartsModel
            ("Температура подачи",SKColors.Orange,"Температура обратки",SKColors.Red,"Давление подачи",SKColors.Blue,"Давление обратки", SKColors.BlueViolet, "One_Charts");
            viewCharts_2 = new ViewChartsModel
             ("Температура котла 1", SKColors.Red, "Температура котла 2", SKColors.Orange, "Температура котла 3", SKColors.Yellow, "Температура котла 4", SKColors.Green, "Two_Charts");
            StartTimer();

        }



        public async Task UpdateCharts()
        {
            await Task.WhenAll(
                viewCharts.UpdateChartsPointsAsync(
                    GetParameterValuesSafe(0),
                    GetParameterValuesSafe(1),
                    GetParameterValuesSafe(2),
                    GetParameterValuesSafe(3)
                ),
                viewCharts_2.UpdateChartsPointsAsync(
                    GetParameterValuesSafe(4),
                    GetParameterValuesSafe(5),
                    GetParameterValuesSafe(6),
                    GetParameterValuesSafe(7)
                ),
                viewCharts.AddAllPoint(),
                viewCharts_2.AddAllPoint()
            );
        }
        private List<ParameterValue> GetParameterValuesSafe(int index)
        {
            if (appState.ChartsParameters == null || index >= appState.ChartsParameters.Count)
                return new List<ParameterValue>();

            return appState.ChartsParameters[index]?.Values ?? new List<ParameterValue>();
        }
        private void StartTimer()
        {
            
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _timer.Tick += async (sender, e) => await UpdateCharts();
            _timer.Start();
        }


    }
}
