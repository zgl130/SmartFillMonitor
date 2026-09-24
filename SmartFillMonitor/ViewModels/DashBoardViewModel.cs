using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using SmartFillMonitor.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SmartFillMonitor.ViewModels
{
    public partial class DashBoardViewModel : ObservableObject
    {
        [ObservableProperty]
        private int actualCount;

        [ObservableProperty]
        private int targetCount;

        [ObservableProperty]
        private double currentTemp;

        [ObservableProperty]
        private double settingTemp;

        [ObservableProperty]
        private double runningTime;

        [ObservableProperty]
        private string deviceStatus;

        [ObservableProperty]
        private double currentCycleTime;

        [ObservableProperty]
        private double standarCycleTime;

        [ObservableProperty]
        private double liquidLevel;

        [ObservableProperty]
        private bool valueOpen=true;

        [ObservableProperty]
        private SeriesCollection tempLiveCharts;

        public ObservableCollection<AlarmUiModel> RecentAlarms { get; set; } = new ObservableCollection<AlarmUiModel>();

        public DashBoardViewModel()
        {
            tempLiveCharts = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title="温度趋势",
                    Values=new  ChartValues<double>(),
                    Fill=Brushes.Gray,
                    Stroke=Brushes.Blue,
                    StrokeThickness=1,
                }
            };
        }

        [RelayCommand]
        private async Task StartProdution()
        {

        }

        [RelayCommand]
        private async Task StopProdution()
        {

        }

        [RelayCommand]
        private async Task ResetProdution()
        {

        }


    }
}
