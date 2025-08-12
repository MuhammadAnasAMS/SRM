using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIA_Admin_Dashboard.Models.AuthModels
{
    [Table("Registration_Requests")]
    public class Registration_Requests
    {
        [Key]
        [Column("reg_id")]
        public int RegId { get; set; }

        [Required]
        [Column("pno")]
        [StringLength(50)]
        public string PNO { get; set; }

        [Column("mobile_confirmed")]
        public bool? MobileConfirmed { get; set; }

        [Column("email_sent")]
        public bool? EmailSent { get; set; }

        [Column("otp_verified")]
        public bool? OtpVerified { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [Column("ip_address")]
        [StringLength(50)]
        public string IPAddress { get; set; }
    }
}
