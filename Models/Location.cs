using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIA_Admin_Dashboard.Models
{
    [Table("Locations")] // Matches the actual SQL table name
    public class Location
    {
        [Key]
        [Column("sno")]
        public int Sno { get; set; }  // Primary Key

        [Column("Location_ID")]
        public string LocationID { get; set; }

        [Column("Location_Description")]
        public string LocationDescription { get; set; }

        [Column("Program_id")]
        public int? ProgramId { get; set; } // Nullable if DB allows nulls

        [ForeignKey("ProgramId")]
        public virtual Program Program { get; set; }
    }
}
