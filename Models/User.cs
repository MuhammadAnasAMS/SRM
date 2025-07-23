using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIA_Admin_Dashboard.Models
{
    public enum EmployeeType
    {
        Regular,
        DailyWages
    }

    public class User
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("employee_id")]
        public string EmployeeId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("mobile")]
        public string Mobile { get; set; }

        [Column("role")]
        public string Role { get; set; }

        [Column("department_id")]
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("is_first_login")]
        public bool? IsFirstLogin { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("designation")]
        public string Designation { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("employee_type")]
        public string EmployeeType { get; set; }  // "Regular" or "DailyWages"
    }
}
