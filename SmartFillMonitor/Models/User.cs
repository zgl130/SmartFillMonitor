using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.Models
{
    [Table(Name = "Users")]
    [Index("idx_unique_username", "UserName", true)]
    public class User
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        [Column(StringLength = 50, IsNullable = false)]
        public string UserName { get; set; }

        [Column(StringLength = 50)]
        public string DisplayName { get; set; }

        [Column(StringLength = 120, IsNullable = false)]
        public string PassWordHash { get; set; }

        [Column(MapType = typeof(int))]
        public Role Role { get; set; }

        public bool IsDisable { get; set; } = false;

        public DateTime CreateAt { get; set; } = DateTime.Now;

        public DateTime LastLoginTime { get; set; }

        public string RoleName => Role switch
        {
            Role.Admin => "管理员",
            Role.Engineer => "工程师",
            Role.Operator => "操作员",
            _ => "未知",
        };

        //public string RoleName
        //{
        //    get
        //    {
        //        switch (Role)
        //        {
        //            case Role.Admin: return "管理员";
        //            case Role.Engineer: return "工程师";
        //            case Role.Operator: return "操作员";
        //            default: return "未知";
        //        }
        //    }
        //}

    }

    public enum Role
    {
        [Description("管理员")]
        Admin = 0,
        [Description("工程师")]
        Engineer = 1,
        [Description("操作员")]
        Operator = 2,
    }
}
