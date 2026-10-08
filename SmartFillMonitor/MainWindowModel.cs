using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using SmartFillMonitor.ViewModels;
using SmartFillMonitor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SmartFillMonitor
{
    //类必须设置为partial
    public partial class MainWindowModel : ObservableObject
    {
        [ObservableProperty]
        private string currentTime;//CommunityToolkit.Mvvm这个框架会自动生成一个CurrentTime，对应View界面绑定使用。
        private readonly DispatcherTimer _timer;

        [ObservableProperty]
        private object mainContent;
        private readonly IServiceProvider _serviceProvider;//用于从IOC容器中获取ViewModel的单例

        [ObservableProperty]
        private string currentBatchNo;

        [ObservableProperty]
        private string currentUserDisplayName = "未登录";

        [ObservableProperty]
        private bool isUserLoggedIn;

        [ObservableProperty]
        private bool isAdmin;

        [ObservableProperty]
        private bool isPlcConnected;

        [ObservableProperty]
        private LightState indicatorState = LightState.Off;


        public MainWindowModel(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;

            PlcServices.ConnectionChanged += (x,connected)=> IsPlcConnected = connected;
            PlcServices.DataReceviced += PlcServices_DataReceviced;
            UserService.LoginStateChanged += User =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateUser(User);
                });

            };
            UpdateUser(UserService.CurrentUser);
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
            Navgiate("DashBoard");//软件启动即进入DashBoard界面
        }

        private void UpdateUser(User user)
        {
            if (user == null)
            {
                CurrentUserDisplayName = "未登录";
                IsUserLoggedIn = false;
                IsAdmin = false;
            }
            else
            {
                CurrentUserDisplayName = user.UserName;
                IsUserLoggedIn = true;
                IsAdmin = user.Role==Role.Admin;
            }
            
        }

        private void PlcServices_DataReceviced(object? sender, DeviceState e)
        {
            currentBatchNo = e.BarCode;
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        }

        #region Navigation Command
        //固定写法CommunityToolkit.Mvvm这个框架会自动生成一个NavgiateToDashBoardCommand，对应View界面绑定使用。
        //[RelayCommand]
        //private void NavgiateToDashBoard()
        //{
        //    MainContent = _serviceProvider.GetRequiredService<DashBoardViewModel>();
        //}

        [RelayCommand]
        private void Navgiate(string? destination)
        {
            if (string.IsNullOrEmpty(destination))
                return;
            switch (destination)
            {
                case "DashBoard":
                    MainContent = _serviceProvider.GetRequiredService<DashBoardViewModel>();
                    break;
                case "DataQuery":
                    MainContent = _serviceProvider.GetRequiredService<DataQueryViewModel>();
                    break;
                case "Logs":
                    MainContent = _serviceProvider.GetRequiredService<LogsViewModel>();
                    break;
                case "Alarms":
                    MainContent = _serviceProvider.GetRequiredService<AlarmsViewModel>();
                    break;
                case "Setting":
                    MainContent = _serviceProvider.GetRequiredService<SettingViewModel>();
                    break;
                default:
                    break;
            }
        }

        #endregion

        [RelayCommand]
        private void Login()
        {
            var loginWin = new LoginWindow()
            {
                Owner = Application.Current.MainWindow
            };
            loginWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result=loginWin.ShowDialog();
            UpdateUser(UserService.CurrentUser);

        }

        [RelayCommand]
        private void ExecuteExit()
        {
            var result=MessageBox.Show("确定退出系统吗","退出系统",MessageBoxButton.YesNo);
            if (result==MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }


    }
}
