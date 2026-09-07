using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SmartFillMonitor
{
    public partial class MainWindowModel : ObservableObject
    {
        private readonly DispatcherTimer _timer;
        [ObservableProperty]
        private string currentTime;//CommunityToolkit.Mvvm这个框架会自动生成一个CurrentTime，对应View界面绑定使用。

        public MainWindowModel()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
