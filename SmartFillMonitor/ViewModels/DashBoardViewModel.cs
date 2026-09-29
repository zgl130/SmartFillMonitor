using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using SmartFillMonitor.Services.Logs;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private string deviceStatus = "自动运行";

        [ObservableProperty]
        private double currentCycleTime;

        [ObservableProperty]
        private double standarCycleTime;

        [ObservableProperty]
        private double liquidLevel;

        [ObservableProperty]
        private bool valueOpen = true;

        [ObservableProperty]
        private SeriesCollection tempLiveCharts;

        private string lastBarcode = string.Empty;

        public ObservableCollection<AlarmUiModel> RecentAlarms { get; set; } = new ObservableCollection<AlarmUiModel>();

        public DashBoardViewModel()
        {
            PlcServices.DataReceviced += PlcServices_DataReceviced;
            AlarmsServices.AlarmRecovered += AlarmsServices_AlarmRecovered;
            TempLiveCharts = new SeriesCollection
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

        private void AlarmsServices_AlarmRecovered(object? sender, AlarmRecord e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RecentAlarms.Insert(0, AlarmUiModel.FormRecord(e));
                if (RecentAlarms.Count > 10)
                {
                    RecentAlarms.RemoveAt(RecentAlarms.Count - 1);
                }
            });
        }

        private void PlcServices_DataReceviced(object? sender, DeviceState e)
        {
            _ = Task.Run(async () =>
            {
                ActualCount = e.ActualCount;
                TargetCount = e.TargetCount;
                CurrentTemp = e.CurrentTemp;
                SettingTemp = e.SettingTemp;
                RunningTime = e.RunningTime;
                CurrentCycleTime = e.CurrentCycleTime;
                StandarCycleTime = e.StandarCycleTime;
                LiquidLevel = e.LiquidLevel;
                valueOpen = e.ValueOpen;
                var barCode = e.BarCode ?? string.Empty;
                if (!string.IsNullOrEmpty(barCode) && barCode != lastBarcode)
                {
                    lastBarcode = barCode;
                    var record = new ProductionRecord
                    {
                        Time = DateTime.Now,
                        BatchNo = barCode,
                        SettingTemp = e.SettingTemp,
                        ActualCount = e.ActualCount,
                        ActualTemp = e.CurrentTemp,
                        TragetCount = e.TargetCount,
                        IsNG = false,
                        CycleTime = e.CurrentCycleTime,
                        Operator = ""

                    };
                    await DbServices.Fsql.Insert<ProductionRecord>(record).ExecuteAffrowsAsync();
                }


            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                TempLiveCharts[0].Values.Add(e.CurrentTemp);
                if (TempLiveCharts[0].Values.Count > 40)
                {
                    TempLiveCharts[0].Values.RemoveAt(0);
                }
            });
        }

        [RelayCommand]
        private async Task StartProdution()
        {
            try
            {
                DeviceStatus = "启动中";
                await PlcServices.WriteCommandAsync("Start", true);
                await Task.Delay(2000);
                DeviceStatus = "运行中";
                LogServices.Info("发送启动命令到PLC");
            }
            catch (Exception ex)
            {
                DeviceStatus = "启动失败";
                LogServices.Error($"启动失败", ex);
            }
        }

        [RelayCommand]
        private async Task StopProdution()
        {
            try
            {
                DeviceStatus = "停止中";
                await PlcServices.WriteCommandAsync("Stop", true);
                await Task.Delay(2000);
                DeviceStatus = "停止中";
                LogServices.Info("发送停止命令到PLC");
            }
            catch (Exception ex)
            {
                DeviceStatus = "停止失败";
                LogServices.Error($"停止失败", ex);
            }
        }

        [RelayCommand]
        private async Task ResetProdution()
        {
            try
            {
                DeviceStatus = "复位中";
                await PlcServices.WriteCommandAsync("Stop", true);
                await Task.Delay(2000);
                await PlcServices.WriteCommandAsync("Reset", true);
                DeviceStatus = "已就绪";
                LogServices.Info("发送复位命令到PLC");
            }
            catch (Exception ex)
            {
                DeviceStatus = "复位失败";
                LogServices.Error($"复位失败", ex);
            }
        }


    }
}
