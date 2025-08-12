using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIA_Admin_Dashboard.Models
{
    [Table("Role")]
    public class Role
    {
        [Key]
        [Column("RoleID")]
        public int RoleID { get; set; }

        [Column("Role_Name")]
        [Required]
        [Display(Name = "Role Name")]
        public string Role_Name { get; set; }

        [Column("Privilege")]  // Changed from "Privileges" to "Privilege" to match your DB
        public string Privilege { get; set; }

        [Column("program_id")]  // Added this field as it exists in your DB
        public int? program_id { get; set; }

        // These fields don't exist in your current DB schema, so removing them
        // If you need them, you'll need to add these columns to your database
        [NotMapped]
        public int UserCount { get; set; } = 0;

        [NotMapped]
        public DateTime? CreatedDate { get; set; }

        [NotMapped]
        public DateTime? LastUpdate { get; set; }

        [NotMapped]
        public bool IsActive { get; set; } = true;
    }

    public class RoleViewModel
    {
        public Role Role { get; set; }
        public string[] SelectedPrivileges { get; set; }
    }
}