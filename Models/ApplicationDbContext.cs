using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace PIA_Admin_Dashboard.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        : base("name=PIA_SRM_Connection") { }

        public DbSet<Department> Departments { get; set; }
    }
}