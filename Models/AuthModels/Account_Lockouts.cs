using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PIA_Admin_Dashboard.Models.AuthModels
{
    public class Account_Lockouts
    {
        [Key]
        public int lockout_id { get; set; }

        [Required]
        public string agent_uid { get; set; }

        public string lockout_type { get; set; }

        public string reason { get; set; }

        public DateTime locked_at { get; set; } = DateTime.Now;

        public DateTime? unlocks_at { get; set; }

        public string ip_address { get; set; }
    }
}