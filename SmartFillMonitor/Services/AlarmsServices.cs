using SmartFillMonitor.Models;
using SmartFillMonitor.Services.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents.DocumentStructures;

namespace SmartFillMonitor.Services
{
    /// <summary>
    /// 系统管理或者设备的报警，并且提供了操作数据的一些方法
    /// </summary>
    public class AlarmsServices
    {
        /// <summary>
        /// 报警触发事件，用于订阅事件播放或者弹窗
        /// </summary>
        public static event EventHandler<AlarmRecord> AlarmTriggered;

        /// <summary>
        /// 报警消除事件，用于订阅事件消除报警红灯
        /// </summary>
        public static event EventHandler<AlarmRecord> AlarmRecovered;

        /// <summary>
        /// 触发报警
        /// </summary>
        /// <param name="alarmRecord"></param>
        /// <returns></returns>
        public static async Task TriggerAlarmsAsync(AlarmRecord alarmRecord)
        {
            try
            {
                bool isAlareadyActive = await DbServices.Fsql.Select<AlarmRecord>()
                              .Where(a => a.AlarmCode == alarmRecord.AlarmCode && a.IsActive)
                              .AnyAsync();

                if (isAlareadyActive) return;   //已经存在报警了，就直接返回
                                                //
                await DbServices.Fsql.Insert<AlarmRecord>().ExecuteAffrowsAsync();
                LogServices.Warn($"触发报警：{alarmRecord.AlarmCode}，严重级别：{alarmRecord.AlarmSeverity}，消息：{alarmRecord.Message}");
                AlarmTriggered?.Invoke(null, alarmRecord);
            }
            catch (Exception ex)
            {
                LogServices.Error($"触发报警异常：{ex.Message}", ex);
            }

        }

        /// <summary>
        /// 报警恢复（比如PLC信号变为正常信号）
        /// </summary>
        /// <param name="alarmRecord"></param>
        /// <returns></returns>
        public static async Task RecoverAlarmAsync(AlarmCode alarmCode)
        {
            try
            {
                var activeAlarm = await DbServices.Fsql.Select<AlarmRecord>()
                              .Where(a => a.AlarmCode == alarmCode && a.IsActive)
                              .FirstAsync();

                if (activeAlarm == null) return;   //没有报警，就直接返回

                activeAlarm.IsActive = false;//
                activeAlarm.EndTime = DateTime.Now;
                activeAlarm.DurationSeconds = (activeAlarm.EndTime - activeAlarm.StartTime).TotalSeconds;

                await DbServices.Fsql.Update<AlarmRecord>().SetSource(activeAlarm).UpdateColumns(a => new { a.IsActive, a.EndTime, a.DurationSeconds }).ExecuteAffrowsAsync();

                LogServices.Info($"报警恢复：{activeAlarm.AlarmCode}，严重级别：{activeAlarm.AlarmSeverity}，消息：{activeAlarm.Message}");
                AlarmRecovered?.Invoke(null, activeAlarm);
            }
            catch (Exception ex)
            {
                LogServices.Error($"恢复异常报警{alarmCode}", ex);
            }
        }

        /// <summary>
        /// 人工确认报警（比如操作员点击了确认按钮）
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> AcknowledgeAlarmAsync(long alarmid, string operatorname)
        {
            try
            {
                var result = await DbServices.Fsql.Update<AlarmRecord>()
                   .Set(a => a.IsActive, true)
                   .Set(a => a.AckTime, DateTime.Now)
                   .Set(a => a.AckUser, operatorname)
                   .Where(a => a.Id == alarmid && !a.IsActive)
                   .ExecuteAffrowsAsync();

                if (result > 0)
                {
                    LogServices.Info($"报警已确认：ID={alarmid} by {operatorname}");
                    return true;
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                LogServices.Info($"报警已确认：ID={alarmid} by {operatorname}");
                return false;
            }

        }

        /// <summary>
        /// 获取当前实时的报警
        /// </summary>
        /// <returns></returns>
        public static async Task<List<AlarmRecord>> GetActiveAlarmsAsync()
        {
            try
            {
                return await DbServices.Fsql.Select<AlarmRecord>()
                               .Where(a => a.IsActive)
                               .OrderByDescending(a => a.StartTime)
                               .ToListAsync();
            }
            catch (Exception ex)
            {
                LogServices.Error($"获取当前实时报警异常：{ex.Message}", ex);
                return new List<AlarmRecord>();
            }

        }


        /// <summary>
        /// 获取历史报警记录（带分页）支持分页，时间范围，严重级别过滤
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static async Task<(List<AlarmRecord>, long)> GetHistoryAlarmsAsync(int pageIndex, int pageSize, DateTime? startTime = null, DateTime? endTime = null, AlarmSeverity alarmSeverity = AlarmSeverity.All)
        {
            try
            {
                var query = DbServices.Fsql.Select<AlarmRecord>();
                if (startTime.HasValue)
                {
                    query = query.Where(a => a.StartTime >= startTime.Value);
                }
                if (endTime.HasValue)
                {
                    query = query.Where(a => a.StartTime <= endTime.Value);
                }
                if (alarmSeverity != AlarmSeverity.All)
                {
                    query = query.Where(a => a.AlarmSeverity == alarmSeverity);
                }

                var total = await query.CountAsync();
                var list = await query.OrderByDescending(a => a.StartTime)
                               .Page(pageIndex, pageSize)
                               .ToListAsync();
                return (list, total);
            }
            catch (Exception ex)
            {
                LogServices.Error($"查询历史报警记录异常：{ex.Message}", ex);
                return (new List<AlarmRecord>(), 0);
            }
        }
    }
}
