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
        public ActionResult Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            using (var db = new ApplicationDbContext())
            {
                // ViewBag for display
                ViewBag.FromDate = fromDate?.ToString("dd-MMM-yyyy") ?? "Beginning";
                ViewBag.ToDate = toDate?.ToString("dd-MMM-yyyy") ?? "Now";

                // Filtered query
                var requestQuery = db.Request_Master.AsQueryable();

                if (fromDate.HasValue)
                    requestQuery = requestQuery.Where(r => r.RequestDate >= fromDate);

                if (toDate.HasValue)
                    requestQuery = requestQuery.Where(r => r.RequestDate <= toDate);

                var requests = requestQuery.ToList().Select(r => new RequestItem
                {
                    RequestID = r.RequestID,
                    Subject = r.ReqSummary,
                    RequestDate = r.RequestDate ?? DateTime.MinValue,
                    ForwardTo = r.ForwardTo,
                    Site = r.PArea,
                    Status = r.Status,
                    PendingSince = r.ForwardedDate.HasValue
                                   ? (DateTime.Now - r.ForwardedDate.Value).Days + " days"
                                   : "N/A"
                }).ToList();

                var stats = new DashboardStats
                {
                    Queue = requestQuery.Count(r => r.Status == "Queue"),
                    Forwarded = requestQuery.Count(r => r.Status == "Forwarded"),
                    Resolved = requestQuery.Count(r => r.Status == "Resolved"),
                    Closed = requestQuery.Count(r => r.Status == "Closed")
                };

                var model = new DashboardViewModel
                {
                    Requests = requests,
                    Stats = stats,
                    OpenRequests = requestQuery.Count(r => r.Status == "Open"),
                    PendingRequests = requestQuery.Count(r => r.Status == "Pending")
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
