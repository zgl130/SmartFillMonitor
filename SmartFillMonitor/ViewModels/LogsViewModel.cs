using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using SmartFillMonitor.Services.Logs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartFillMonitor.ViewModels
{
    public partial class LogsViewModel : ObservableObject
    {
        public LogsViewModel()
        {
            _ = LoadLogsAsync();
        }

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today.AddDays(1).AddSeconds(-1);

        [ObservableProperty]
        private string _selectLevel = "All";

        public ObservableCollection<string> LogLevel { get; set; } = new ObservableCollection<string>
        {
        "All",
        "Debug",
        "Information",
        "Warning",
        "Error"
        };

        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private ObservableCollection<SystemLog> _logs = new ObservableCollection<SystemLog>();

        [ObservableProperty]
        private bool _isBusy;//数据库操作是否忙碌

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _pageIndex;

        private const int pageSize = 50;//每页显示50条数据

        [RelayCommand]
        private async Task SearchAsync()
        {
            PageIndex = 1;
            await LoadLogsAsync();
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            var query = BuildQuery();
            var allData = await query.OrderByDescending(x => x.TimeStamp).ToListAsync();
            if (allData.Count == 0)
            {
                MessageBox.Show("没有数据可以导出");
                return;
            }
            var lines = new List<string>()
            {
                "时间,等级,内容,异常"
            };
            //x.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss")   // 等价写法
            //lines.AddRange(allData.Select(x => $"{x.TimeStamp:yyyy-MM-dd HH:mm:ss},{x.Level},{x.RenderedMessage},{x.Exception}"));//这种写法对可能导致异常的\ \r \n等没有转换，查查C#转义字符
            lines.AddRange(allData.Select(x => $"{x.TimeStamp:yyyy-MM-dd HH:mm:ss},{x.Level},\"{x.RenderedMessage.Replace("\"", "\"\"")}\",\"{x.Exception?.Replace("\n", "")}\""));
            var path = $"Logs_Export_{DateTime.Now:yyyyMMddHHmmss}.csv";
            await File.WriteAllLinesAsync(path, lines,Encoding.UTF8);
            MessageBox.Show($"日志导出到文件{Path.GetFullPath(path)}");

        }

        [RelayCommand]
        private async Task PrevPageAsync()
        {
            if (PageIndex <= 1) return;
            PageIndex--;
            await LoadLogsAsync();
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (Logs.Count < pageSize) return;//已经是最后一页
            PageIndex++;
            await LoadLogsAsync();
        }

        private async Task LoadLogsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var query = BuildQuery();
                TotalCount = (int)await query.CountAsync();
                var data = await query.OrderByDescending(x => x.TimeStamp).Page(PageIndex, pageSize)
                    .ToListAsync();
                Logs = new ObservableCollection<SystemLog>(data);
            }
            catch (Exception ex)
            {
                LogServices.Error($"加载日志失败{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }

        }

        private FreeSql.ISelect<SystemLog> BuildQuery()
        {
            var query = DbServices.Fsql.Select<SystemLog>();
            // Sqlite存储的时间格式为ISO 8601，如 2026-09-18T22:16:56.692（带T带毫秒）
            // 必须用 datetime() 把两边统一成 'yyyy-MM-dd HH:mm:ss' 再比较。
            //
            // 【不能】用 lambda 直接比 x.TimeStamp >= start && x.TimeStamp < end：
            //   FreeSql 把 DateTime 参数序列化成空格分隔 '2026-09-18 23:59:59'，
            //   而存储是 'T' 分隔。字符串逐字符比较时第11位 'T'(0x54) > ' '(0x20)，
            //   当 end 与数据同一天时 '2026-09-18T22:16:56' > '2026-09-18 23:59:59'，
            //   导致 < end 全不成立 → 当天数据全漏（实测命中 0 条，datetime() 命中 16 条）。
            //   lambda 直比仅在"按天查（end 是次日0点）"时碰巧正确（日期位先分胜负）。
            //
            // 当前 UI 用 HandyControl DateTimePicker 能选到秒，直接用选中值：
            // datetime() 两边都截断毫秒，<= end 时 23:59:59.999 也能命中。
            var start = StartDate.ToString("yyyy-MM-dd HH:mm:ss");
            var end = EndDate.ToString("yyyy-MM-dd HH:mm:ss");
            query = query.Where($"datetime(\"Timestamp\")>=datetime('{start}') AND datetime(\"Timestamp\")<=datetime('{end}')");

            //var start = StartDate;//不行，虽然安全但是，时间格式不一致
            //var end = EndDate;
            //query = query.Where(x => x.TimeStamp >= start && x.TimeStamp < end);

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => x.RenderedMessage.Contains(SearchText));
            }
            // 注意：SelectLevel 的值来自 LogLevel 集合（"All","Debug",...），首字母大写、后面小写。
            // 之前写 "ALL" 全大写做比较，"All" != "ALL" 永远成立，导致这里每次都错误地加上
            // Level.Contains("All") 过滤，而 Serilog 实存的 Level 是 "Information/Debug/Error"，
            // 全不含 "All"，结果查出 0 条。改成不区分大小写判断哨兵值 "All"。
            if (!string.IsNullOrEmpty(SelectLevel) &&
                !string.Equals(SelectLevel, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Level == SelectLevel);   // 精确匹配，比 Contains 更准
            }

            return query;
        }

    }
}
