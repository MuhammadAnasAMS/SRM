using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PIA_Admin_Dashboard.Models.AuthModels
{
    public class Login_Attempts
    {
        [Key]
        public int attempt_id { get; set; }

        [Required]
        public string email { get; set; }

        public DateTime attempt_time { get; set; } = DateTime.Now;

        public string ip_address { get; set; }

        public string user_agent { get; set; }

        public bool was_successful { get; set; }
    }
}