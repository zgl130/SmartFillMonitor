using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.Models
{
    [Table(Name = "ProductionRecord")]
    public class ProductionRecord
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        [Column(StringLength =50)]
        public string BatchNo { get; set; }

        public double SettingTemp { get; set; }

        public double ActualTemp { get; set; }

        public double FillWeigt { get; set; }

        public int ActualCount { get; set; }

        public int TragetCount { get; set; }

        public bool IsNG { get; set; }

        [Column(StringLength = 100)]
        public string? NGReason { get; set; }

        public double CycleTime { get; set; }

        [Column(StringLength = 100)]
        public string? Operator { get; set; }


    }
}
