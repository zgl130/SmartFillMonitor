using FreeSql.DataAnnotations;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.Models
{
    [Table(Name= "SystemLog" ,DisableSyncStructure =true)]
    public class SystemLog
    {
        [Column(Name ="id",IsPrimary =true ,IsIdentity =true)]
        public int ID { get; set; }

        [Column(Name ="Timestamp")]
        public DateTime TimeStamp { get; set; }

        [Column(StringLength =50)]
        public string Level { get; set; }

        [Column(StringLength = 1000)]
        public string   Exception { get; set; }

        [Column(StringLength = 50)]
        public string RenderedMessage { get; set; }

        public string Properties { get; set; }

    }
}
