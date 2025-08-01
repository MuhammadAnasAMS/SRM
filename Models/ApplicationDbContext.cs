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
        public DbSet<User> Users { get; set; }
        public DbSet<Program> Programs { get; set; }

        public DbSet<Agent> Agents { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<History> HistoryLogs { get; set; }
        public virtual DbSet<Request_Master> Request_Master { get; set; }
    }
}