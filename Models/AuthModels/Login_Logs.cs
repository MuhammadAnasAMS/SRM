using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PIA_Admin_Dashboard.Models.AuthModels
{
    public class Login_Logs
    {
        [Key]
        public int log_id { get; set; }

        public string agent_uid { get; set; }

        public string login_method { get; set; } // 'OTP' or 'Password'

        public DateTime login_time { get; set; } = DateTime.Now;

        public string ip_address { get; set; }

        public string user_agent { get; set; }

        public bool was_successful { get; set; }
    }
}