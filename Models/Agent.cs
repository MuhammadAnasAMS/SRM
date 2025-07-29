using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIA_Admin_Dashboard.Models
{
    [Table("Agent")] // Must match DB table
    public class Agent
    {
        [Column("sno")]
        public int AgentId { get; set; }

        [Column("pno")]
        public string PNO { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("create_dt")]
        public DateTime? CreatedAt { get; set; }

        [Column("close_dt")]
        public DateTime? ClosedAt { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("Close_Remarks")]
        public string CloseRemarks { get; set; }

        [Column("isAdministrator")]
        public string IsAdministrator { get; set; }

        [Column("LastLogin_DateTime")]
        public DateTime? LastLoginDateTime { get; set; }

        [Column("LastLogin_IP")]
        public string LastLoginIP { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("mobile")]
        public string Mobile { get; set; }

        [Column("workarea")]
        public string WorkArea { get; set; }

        [Column("gid")]
        public int? Gid { get; set; }

        [Column("Privilege")]
        public string Privilege { get; set; }

        [Column("roleid")]
        public int? RoleId { get; set; }

        [Column("UserType")]
        public string UserType { get; set; }

        [Column("uid")]
        public string AgentUid { get; set; }

        [Column("MobileOperator")]
        public string MobileOperator { get; set; }

        [Column("program_id")]
        public int? ProgramId { get; set; }

        [Column("LastUpdate")]
        public DateTime? LastUpdate { get; set; }
    }
}