using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIA_Admin_Dashboard.Models
{
    [Table("Program_Setup")] // Exact table name in their DB
    public class Program
    {
        [Column("Program_ID")]
        public int ProgramId { get; set; }

        [Column("Program_Name")]
        public string Name { get; set; }

        [Column("uid")]
        public string ProgramUid { get; set; }
    }
}
