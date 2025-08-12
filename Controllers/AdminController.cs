using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;

namespace PIA_Admin_Dashboard.Controllers
{
    public class AdminController : AuthenticatedController
    {
        public ActionResult Dashboard()
        {
            using (var db = new ApplicationDbContext())
            {
                var model = new DashboardStatusViewModel
                {
                    Queue = db.Request_Master.Count(r => r.Status == "Q"),
                    Forwarded = db.Request_Master.Count(r => r.Status == "F"),
                    In_Progress = db.Request_Master.Count(r => r.Status == "P"),
                    Resolved = db.Request_Master.Count(r => r.Status == "R"),
                    Closed = db.Request_Master.Count(r => r.Status == "C")
                };

                return View(model);
            }
        }

        public ActionResult Departments()
        {
            ViewBag.ScreenTitle = "Departments Management";
            return View();
        }

        public ActionResult Users()
        {
            ViewBag.ScreenTitle = "User Management";
            return View();
        }

        public ActionResult Complaints()
        {
            ViewBag.ScreenTitle = "Complaints Handling";
            return View();
        }

        public ActionResult Logs()
        {
            var logs = new List<LogEntry>
            {
        new LogEntry { EmployeeId = "EMP001", EmployeeName = "Ali Raza", IpAddress = "192.168.1.10", Status = "Login", DateTime = DateTime.Now },
        new LogEntry { EmployeeId = "EMP002", EmployeeName = "Zara Khan", IpAddress = "192.168.1.20", Status = "Logout", DateTime = DateTime.Now.AddMinutes(-30) },
        // Add more dummy data
            };

            return View(logs);
        }
    }
}
