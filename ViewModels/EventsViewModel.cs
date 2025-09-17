using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public partial class EventsViewModel : ObservableObject
    {

        public ObservableCollection<EventData> Events_Items { get; set; } = new();
        public IRelayCommand ApplyFilter => AppStateViewModel.ApplyFilterCommand;
        public DateTime EventDate
        {
            get => AppStateViewModel.EventDate;
            set
            {
                if (AppStateViewModel.EventDate != value)
                {
                    AppStateViewModel.EventDate = value;
                    OnPropertyChanged();
                }
            }
        }
        AppStateViewModel AppStateViewModel { get; set; }
        public EventsViewModel(AppStateViewModel app)
        {
            AppStateViewModel = app;
            Events_Items = AppStateViewModel.Events_Filter_Items;
        }



        








    }



}
