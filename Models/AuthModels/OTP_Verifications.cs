using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIA_Admin_Dashboard.Models.AuthModels
{
    [Table("OTP_Verifications")]
    public class OTP_Verifications
    {
        [Key]
        public int otp_id { get; set; }

        public string agent_uid { get; set; }

        public string otp_code { get; set; }

        public bool is_used { get; set; } = false;

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime? expires_at { get; set; }

        public string purpose { get; set; }  // 'register' or 'reset'

        public string sent_to_email { get; set; }

        public string ip_address { get; set; }

        public string attempt_source { get; set; }
    }
}
