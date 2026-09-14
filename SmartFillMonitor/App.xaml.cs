using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SmartFillMonitor.ViewModels;
using System.Configuration;
using System.Data;
using System.Security.RightsManagement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SmartFillMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //全局唯一的日志框
        public static RichTextBox LogView = new RichTextBox
        {
            IsReadOnly = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = Brushes.Black,
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Consolas")
        };
        private const string LogTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({ThreadId}) {Message:lj}{NewLine}{Exception}";
        private const string LogPath = "Logs\\log-.txt";//会自动生成Logs文件夹，日志文件名会以log-时间来命名
        private const string DbFilePath = "SmartFillMonitor.db";

        public IServiceProvider ServiceProvider { get; private set; }//公开只读属性，保存已经构建的DI服务，让其他类可以解析到服务
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigLogging();//配置日志
            var services = new ServiceCollection();//创建新的DI服务集合，这是依赖注入第一步。
            ConfigureServices(services);//注入View单例到DI容器
            ServiceProvider = services.BuildServiceProvider();//供外部调用
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void ConfigLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.RichTextBox(LogView, outputTemplate: LogTemplate)
                .WriteTo.Console(outputTemplate: LogTemplate)
                .WriteTo.Async(a=>a.File(LogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: null, outputTemplate: LogTemplate, shared: true))//异步写入
                //.WriteTo.File(LogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: null, outputTemplate: LogTemplate, shared: true)//shared多个实例写入,retainedFileCountLimit: null不限制日志个数
                .WriteTo.SQLite(DbFilePath, tableName: "SystemLog", storeTimestampInUtc: false )
                //.WriteTo.Async(a=>a.SQLite(DbFilePath,tableName: "SystemLog",storeTimestampInUtc:false,retentionPeriod: TimeSpan.FromDays(365)))//retentionPeriod可以把长期保存日志交给数据库，文件保存设置为90天
                .CreateLogger();
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
