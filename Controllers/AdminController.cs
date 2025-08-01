using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;

namespace PIA_Admin_Dashboard.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Dashboard()
        {
            ViewBag.ScreenTitle = "Dashboard";
            return View();
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
