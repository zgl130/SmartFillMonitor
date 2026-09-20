using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using SmartFillMonitor.Services.Logs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        /// <summary>
        /// 端口号
        /// </summary>
        public ObservableCollection<string> PortName { get; set; } = new ObservableCollection<string>();
        /// <summary>
        /// 波特率
        /// </summary>
        public ObservableCollection<int> BaudRate { get; set; } = new ObservableCollection<int>()
        {
            2400,4800,9600,19200,38400,57600,115200
        };
        /// <summary>
        /// 数据位
        /// </summary>
        public ObservableCollection<int> DataBits { get; set; } = new ObservableCollection<int>()
        {
            7,8
        };
        /// <summary>
        /// 校验位
        /// </summary>
        public ObservableCollection<string> Parity { get; set; } = new ObservableCollection<string>(Enum.GetNames(typeof(Parity)));

        /// <summary>
        /// 停止位
        /// </summary>
        public ObservableCollection<string> StopBits { get; set; } = new ObservableCollection<string>(Enum.GetNames(typeof(StopBits)));

        [ObservableProperty] private string selectedPortName = "COM1";
        [ObservableProperty] private int selectedBaudRate = 9600;
        [ObservableProperty] private int selectedDataBits = 8;
        [ObservableProperty] private string selectedParity = "None";
        [ObservableProperty] private string selectedStopBits = "One";
        [ObservableProperty] private bool autoConnect = true;
        [ObservableProperty] private bool alarmSound = true;
        [ObservableProperty] private bool dubugLogMode = true;

        public SettingViewModel()
        {
            RefreashPortList();
            _ = LoadSettings();

        }
        private void RefreashPortList()
        {
            PortName.Clear();
            try
            {
                var ports = PlcServices.GetAvailablePorts ?? SerialPort.GetPortNames();
                foreach (var item in ports)
                {
                    PortName.Add(item);
                }

                //PortName = new ObservableCollection<string>(ports);同等效应
                selectedPortName = PortName.Count > 0 ? PortName[0] : selectedPortName;
            }
            catch (Exception ex)
            {
                LogServices.Error($"获取串口列表失败{ex.Message}");
                PortName.Clear();
                PortName.Add("COM1");
                PortName.Add("COM2");
            }
        }


        private async Task LoadSettings()
        {
            try
            {
                var ds = await ConfigServices.LoadDeviceSettingAsync();
                selectedPortName = ds.PortName;
                selectedBaudRate = ds.BaudRate;
                selectedDataBits = ds.DataBits;
                selectedParity = ds.Parity;
                selectedStopBits = string.IsNullOrEmpty(ds.StopBits) ? "One" : ds.StopBits;
                autoConnect = ds.AutoConnect;
                alarmSound = ds.AlarmSound;
                dubugLogMode = ds.DebugLogMode;
                //return;
            }
            catch (Exception ex)
            {
                //日志加载失败
                LogServices.Warn($"加载失败,使用默认值，原因：{ex.Message}");
            }
        }

        //固定写法CommunityToolkit.Mvvm这个框架会自动生成一个SaveCommand，对应View界面绑定使用。
        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                var model = new DeviceSettings
                {
                    PortName = selectedPortName,
                    BaudRate = selectedBaudRate,
                    DataBits = selectedDataBits,
                    Parity = selectedParity,
                    StopBits = selectedStopBits,
                    AutoConnect = autoConnect,
                    AlarmSound = alarmSound,
                    DebugLogMode = dubugLogMode,
                };

                await ConfigServices.SaveDeviceSettingAsync(model);
            }
            catch (Exception ex)
            {
                LogServices.Error($"保存失败{ex.Message}");
                //保存失败
            }
        }

    }
}
