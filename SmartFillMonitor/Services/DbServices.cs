using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.Services
{
    public class DbServices
    {
        private static readonly object _lock = new object();//锁。保证线程安全

        public static IFreeSql Fsql { get; private set; }

        /// <summary>
        /// 初始化FreeSql
        /// </summary>
        /// <param name="connetionString">数据库连接字符串</param>
        /// <param name="data">数据库类型</param>
        public static void Initialze(string connectionString, FreeSql.DataType dataType = FreeSql.DataType.Sqlite)
        {
            if (Fsql != null) return;
            lock (_lock)
            {
                if (Fsql != null) return;
                Fsql = new FreeSql.FreeSqlBuilder()
                      .UseConnectionString(dataType, connectionString)
                      .UseMonitorCommand(
                    cmd =>
                    {
                        //sql执行前
                    },
                    (cmd,tracelog )=>
                    {
                        Console.WriteLine($"[SQL] {cmd.CommandText}/r/n ->{tracelog}");
                    }
                    )//监听SQL语句执行
                      .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
                      .UseLazyLoading(false)//禁用懒加载，调用时才会实例化
                      .Build();
            }
        }

       

    }
}
