using Microsoft.Extensions.DependencyInjection;
using SmartFillMonitor.ViewModels;
using System.Configuration;
using System.Data;
using System.Security.RightsManagement;
using System.Windows;

namespace SmartFillMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
     public IServiceProvider  ServiceProvider { get; private set; }//公开只读属性，保存已经构建的DI服务，让其他类可以解析到服务
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var services = new ServiceCollection();//创建新的DI服务集合，这是依赖注入第一步。
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();//供外部调用
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<AlarmsViewModel>();
            services.AddSingleton<DashBoardViewModel>();
            services.AddSingleton<DataQueryViewModel>();
            services.AddSingleton<LogsViewModel>();
            services.AddSingleton<SettingViewModel>();
            services.AddSingleton<MainWindowModel>();
        }



    }

}
