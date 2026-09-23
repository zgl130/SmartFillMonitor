using Microsoft.Extensions.DependencyInjection;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using SmartFillMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartFillMonitor.Views
{
    /// <summary>
    /// AlarmsView.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmsView : UserControl
    {
        public AlarmsView()
        {
            InitializeComponent();

            var app = Application.Current as App;
            if (app != null)
                this.DataContext = app.ServiceProvider.GetRequiredService<AlarmsViewModel>();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await AlarmsServices.TriggerAlarmsAsync(new AlarmRecord
            {
                AlarmCode = AlarmCode.CommunicationError,
                Message = "Plc 通信失败，请检查",
                AlarmSeverity = AlarmSeverity.Error,
            });
        }
    }
}
