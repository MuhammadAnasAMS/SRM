using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
namespace PIA_Admin_Dashboard.Models
{
    public class Department
    {
        [Column("department_id")]
        public int DepartmentId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } 
    }

}