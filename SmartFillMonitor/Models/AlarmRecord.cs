using FreeSql.DataAnnotations;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.Models
{
    #region 实体类模型
    //特性属性赋值的特殊语法，不使用类初始化
    [Table(Name = "AlarmRecord")]
    public class AlarmRecord
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        //报警类型
        public AlarmCode AlarmCode { get; set; }

        //报警级别
        public AlarmSeverity AlarmSeverity { get; set; }

        //报警开始时间
        public DateTime StartTime { get; set; } = DateTime.Now;

        //报警恢复时间（设备解除故障）
        public DateTime EndTime { get; set; }

        //报警持续时间
        public double? DurationSeconds { get; set; }

        //是否为活动报警（true代表故障，false代表已恢复）
        public bool IsActive { get; set; }

        //是否人工确认
        public bool IsAcknowledged { get; set; }

        //确认时间
        public DateTime? AckTime { get; set; }

        //确认人信息（记录用户名）   
        public string? AckUser { get; set; }

        //动态消息（温度过高，当前100℃）
        public string? Message { get; set; }

        //处理建议（通常从Enum中获取）
        public string? Description { get; set; }
    }

    /// <summary>
    /// 报警级别
    /// </summary>
    public enum AlarmSeverity
    {
        [Description("所有")]
        All = 0,
        [Description("提示")]
        Info = 1,
        [Description("警告")]
        Warning = 2,
        [Description("错误")]
        Error = 3,
        [Description("致命")]
        Critical = 4,
    }

    public enum AlarmCode
    {
        [Description("无")]
        None = 0,
        [Description("原料桶液位过低")]
        LowLiquidLevel = 1001,
        [Description("压缩空气压力偏低")]
        LowAirPresure = 2001,
        [Description("加热温度过高")]
        HighTemperature = 3001,
        [Description("Plc通信故障")]
        CommunicationError = 4001,
        [Description("系统内部错误")]
        SystemError = 5001,

    }
    #endregion

    #region UI视图模型
    //UI视图模型，用于显示在界面
    public class AlarmUiModel:INotifyPropertyChanged
    {
        private long _id;
        private string _code;
        private string _title;
        private string _timeStr;
        private string _description;

        public long Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Code
        {
            get { return _code; }
            set
            {
                if (_code == value) return;
                _code = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string TimeStr
        {
            get { return _timeStr; }
            set
            {
                if (_timeStr == value) return;
                _timeStr = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string? propertyName=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static AlarmUiModel FormRecord(AlarmRecord record)
        {
            var titel = record.AlarmCode.GetDescription();
            return new AlarmUiModel
            {
                Id = record.Id,
                Code = $"E{(int)record.AlarmCode}",//加上前缀，看着像故障码
                Title = titel,
                Description = record.Description,
                TimeStr = record.StartTime.ToString("MM-dd HH:mm:ss"),
            };
        }
    }
    #endregion

    public static class EnumExtentions
    {
        public static string GetDescription(this Enum value)
        {
            var filed = value.GetType().GetField(value.ToString());
            var attribute=Attribute.GetCustomAttribute(filed, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute?.Description ?? value.ToString();
        }
    }
}
