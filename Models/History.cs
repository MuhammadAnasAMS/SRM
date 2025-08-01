using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PIA_Admin_Dashboard.Models
{
    [Table("History")] 
    public class History
    {
        [Key]
        [Column("sno")]
        public int Sno { get; set; }

        [Column("pno")]
        public string PNO { get; set; }

        [Column("date")]
        public DateTime? Date { get; set; }

        [Column("action")]
        public string Action { get; set; }

        [Column("ip_address")]
        public string IPAddress { get; set; }
    }
}