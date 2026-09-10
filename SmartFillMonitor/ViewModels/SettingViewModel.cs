using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        /// <summary>
        /// 端口号
        /// </summary>
        public ObservableCollection<string> PortName { get; } = new ObservableCollection<string>();
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
        [ObservableProperty] private string selectedParity ="None";
        [ObservableProperty] private string selectedStopBits = "One";
        [ObservableProperty]private bool autoConnect = true;
        [ObservableProperty]private bool alarmSound = true;
        [ObservableProperty]private bool dubugLogMode = true;

        //固定写法CommunityToolkit.Mvvm这个框架会自动生成一个SaveCommand，对应View界面绑定使用。
        [RelayCommand]
        private async Task SaveAsync()
        {

        }

        public SettingViewModel()
        {

            
        }

    }
}
