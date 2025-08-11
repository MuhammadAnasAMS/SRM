using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIA_Admin_Dashboard.Models
{
    [Table("Groups")]
    public class Group
    {
        [Key]
        [Column("gid")]
        public int Gid { get; set; }

        [Column("gname")]
        [Required(ErrorMessage = "Group name is required")]
        public string GroupName { get; set; }

        [Column("program_id")]
        public int? ProgramId { get; set; }

        [Column("uid")]
        public string Uid { get; set; }

        [Column("manager_pno")]
        public string ManagerPno { get; set; }

        // Navigation properties
        [ForeignKey("ProgramId")]
        public virtual Program Program { get; set; }

        // Navigation property for manager (based on PNO from Agent table)
        [NotMapped]
        public virtual Agent Manager { get; set; }

        // Computed property for member count (from AgentGroup table)
        [NotMapped]
        public int MemberCount { get; set; }

        // Helper property to get manager name for display
        [NotMapped]
        public string ManagerName { get; set; }
    }
}