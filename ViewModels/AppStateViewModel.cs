using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using Microsoft.Extensions.DependencyInjection;

namespace WpfApp1
{
    public partial class AppStateViewModel : ObservableObject
    {
        private DispatcherTimer _timer;
        HTTP_Client hTTP_Client;
        [ObservableProperty]
        private DateTime eventDate;

        private DateTime CurrentDate;
        public ObservableCollection<EventData> Events_Filter_Items { get; set; } = new();
        [ObservableProperty]
        private ObservableCollection<Parameters> parameters = new();
        [ObservableProperty]
        private ObservableCollection<ParameterDataResponse> chartsParameters = new();

        private List<EventData> eventDatas = new();
        public AppStateViewModel(HTTP_Client http)
        {
            EventDate = DateTime.Now;
            CurrentDate = DateTime.Now;

            hTTP_Client = http;
            StartTimer();
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += async (sender, e) => await UpdateParametersAsync();
            _timer.Start();
        }

        public async Task UpdateParametersAsync()
        {
            var newParameters = await hTTP_Client.Get_Param();
            var newChartsParameters = await hTTP_Client.Get_Last_Param();
            var newEvents = await hTTP_Client.Get_Events();
            Parameters = new ObservableCollection<Parameters>(newParameters);
            ChartsParameters = new ObservableCollection<ParameterDataResponse>(newChartsParameters);
            eventDatas = new List<EventData>(newEvents);
            await ApplyFilter(CurrentDate);
        }


        [RelayCommand]
        public async Task ApplyFilter()
        {
            CurrentDate = EventDate.Date;
            var nextDay = EventDate.Date.AddDays(1);
            var filtered_1 = await Task.Run(() =>
            {
                var filtered_1 = eventDatas
                    .Where(p => p.StartDateTime.Date >= CurrentDate.Date && p.StartDateTime.Date < nextDay.Date);

                return filtered_1.ToArray();
            });

            Events_Filter_Items.Clear();
            foreach (var item in filtered_1)
            {
                Events_Filter_Items.Add(item);
            }
        }

        public async Task ApplyFilter(DateTime date)
        {
            CurrentDate = date;
            var nextDay = CurrentDate.Date.AddDays(1);
            var filtered_1 = await Task.Run(() =>
            {
                var filtered_1 = eventDatas
                    .Where(p => p.StartDateTime.Date >= CurrentDate.Date && p.StartDateTime.Date < nextDay.Date);

                return filtered_1.ToArray();
            });

            Events_Filter_Items.Clear();
            foreach (var item in filtered_1)
            {
                Events_Filter_Items.Add(item);
            }
        }

        


    }
}

