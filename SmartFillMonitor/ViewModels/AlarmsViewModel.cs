using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFillMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.ViewModels
{
    public partial class AlarmsViewModel:ObservableObject
    {
        public AlarmsViewModel()
        {
                
        }

        public ObservableCollection<AlarmUiModel> ActiveAlarms { get; set; }
        public ObservableCollection<AlarmUiModel> HistoryAlarms { get; set; }
        [ObservableProperty]
        private int activeAlarmCount;
        [ObservableProperty]
        private DateTime historyStartDate=DateTime.Today.AddDays(-1);
        [ObservableProperty]
        private DateTime historyEndDate=DateTime.Today;

        [RelayCommand]
        private async Task RefreshAsync()
        {

        }

        [RelayCommand]
        private async Task LoadHistoryAlarmsAsync()
        {

        }

    }
}
