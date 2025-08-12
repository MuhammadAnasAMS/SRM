using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PIA_Admin_Dashboard.Models.AuthModels
{
    public class Agent_Credentials
    {
        [Key]
        public string agent_uid { get; set; }

        [Required]
        public string email { get; set; }

        [Required]
        public string password_hash { get; set; }
        public bool is_active { get; set; } = true;

    }
}