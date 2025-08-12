using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using PIA_Admin_Dashboard.Models.AuthModels;
namespace PIA_Admin_Dashboard.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        : base("name=PIA_SRM_Connection") { }


        public DbSet<Program> Programs { get; set; }

        public DbSet<Group> Groups { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<History> HistoryLogs { get; set; }
        public virtual DbSet<Request_Master> Request_Master { get; set; }

        // Auth Process
        public DbSet<Login_Attempts> Login_Attempts { get; set; }
        public DbSet<Account_Lockouts> Account_Lockouts { get; set; }
        public DbSet<Agent_Credentials> Agent_Credentials { get; set; }
        public DbSet<Login_Logs> Login_Logs { get; set; }
        public DbSet<OTP_Verifications> OTP_Verifications { get; set; }
        public DbSet<Registration_Requests> Registration_Requests { get; set; }

    }
}