using CsvHelper;
using LiveCharts;
using SmartFillMonitor.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.Services
{
    public static class DataService
    {
        public static async Task SaveProductionRecordAsync(ProductionRecord record)
        {
            await DbServices.Fsql.Insert<ProductionRecord>(record).ExecuteIdentityAsync();

        }

        public static async Task<List<ProductionRecord>> QueryRecordAsync(DateTime start, DateTime end)
        {
            return await DbServices.Fsql.Select<ProductionRecord>()
            .Where(r => r.Time >= start && r.Time <= end)
            .ToListAsync();
        }

        public static async Task ExportToCSV(List<ProductionRecord> records, string pathStr)
        {
            //// using 版
            //using (var writer = new StreamWriter(pathStr))
            //{
            //    throw new Exception("出错了");
            //}
            //// 结果：Dispose() 先执行，然后异常继续往外抛
            //// 等价的 try/finally 版
            //var writer = new StreamWriter(pathStr);
            //try
            //{
            //    throw new Exception("出错了");
            //}
            //finally
            //{
            //    writer.Dispose();   // 先执行
            //}
            //// 结果：finally 执行完，异常继续往外抛
            ////using = try + finally（保证释放），不含 catch（不处理异常）。

            await using var writer = new StreamWriter(pathStr);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await csv.WriteRecordsAsync(records);
        }

    }
}
