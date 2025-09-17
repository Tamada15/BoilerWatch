using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WpfApp1
{
    public partial class ShellViewModel: ObservableObject
    {
        public MainViewModel Dashboard { get; }
        public EventsViewModel Events { get; }

        [ObservableProperty]
        private string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private object _currentPageViewModel;
        public object CurrentPageViewModel
        {
            get => _currentPageViewModel;
            set
            {
                if (SetProperty(ref _currentPageViewModel, value))
                {
                    OnPropertyChanged(nameof(IsDashboardActive));
                    OnPropertyChanged(nameof(IsEventsActive));

                }
            }
        }

        public bool IsDashboardActive => CurrentPageViewModel == Dashboard;
        public bool IsEventsActive => CurrentPageViewModel == Events;




        public ShellViewModel(MainViewModel mainViewModel, EventsViewModel _events)
        {
            Dashboard = mainViewModel;
            Events = _events;
            CurrentPageViewModel = Dashboard;
        }

        [RelayCommand]
        private void ShowDashboard() => CurrentPageViewModel = Dashboard;

        [RelayCommand]
        private void ShowEvents() => CurrentPageViewModel = Events;

        





    }
}
