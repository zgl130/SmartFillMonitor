using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using SmartFillMonitor.Services.Logs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartFillMonitor.ViewModels
{
    public partial class AlarmsViewModel:ObservableObject
    {
        public AlarmsViewModel()
        {
            ActiveAlarms = new ObservableCollection<AlarmUiModel>();
            HistoryAlarms= new ObservableCollection<AlarmUiModel>();    
            AlarmsServices.AlarmTriggered += OnAlarmTriggered;
            _ = LoadActiveAlarmsAsync();
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
            await LoadActiveAlarmsAsync();
        }

        [RelayCommand]
        private async Task LoadHistoryAlarmsAsync()
        {
         var records=await AlarmsServices.GetHistoryAlarmsAsync(1,20,HistoryStartDate,HistoryEndDate.AddDays(1),AlarmSeverity.All);
            Application.Current.Dispatcher.Invoke(() =>
            {
                HistoryAlarms.Clear();
                foreach (var item in records.Item1)
                {
                    HistoryAlarms.Add(AlarmUiModel.FormRecord(item));
                }
              
            });
        }

        [RelayCommand]
        private async Task AcknowledgeAlarmAsync(AlarmUiModel alarm)
        {
            try
            {
                if (alarm == null) return;

                var resutl = await AlarmsServices.AcknowledgeAlarmAsync(alarm.Id, "");
                if (resutl)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ActiveAlarms.Remove(alarm);
                        ActiveAlarmCount=ActiveAlarms.Count;
                    });
                    LogServices.Info($"确认报警成功{alarm.Code}");
                }
            }
            catch (Exception ex)
            {
                LogServices.Error($"确认报警异常{alarm.Code}", ex);
            }
            
        }

        private async Task LoadActiveAlarmsAsync()
        {
            try
            {
                var records=await AlarmsServices.GetActiveAlarmsAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ActiveAlarms.Clear();
                    foreach (var item in records)
                    {
                        ActiveAlarms.Add(AlarmUiModel.FormRecord(item));
                    }
                    ActiveAlarmCount = ActiveAlarms.Count();
                });
            }
            catch ( Exception ex)
            {
                LogServices.Error("加载活动报警异常",ex);
            }
        }

        private void OnAlarmTriggered(object? sender, AlarmRecord e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var alarm = AlarmUiModel.FormRecord(e);

                //插到列表头（最新的报警显示在界面最上面）
                ActiveAlarms.Insert(0, alarm);
                ActiveAlarmCount=ActiveAlarms.Count;

                LogServices.Error($"新报警：{alarm.Code}-{alarm.Title}");
            }));
        }

       

    }
}
