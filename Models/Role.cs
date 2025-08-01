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
        public string Role_Name { get; set; }

        [Column("Privilege")]
        public string Privilege { get; set; }

        [Column("program_id")]
        public int? ProgramId { get; set; }
    }
}
