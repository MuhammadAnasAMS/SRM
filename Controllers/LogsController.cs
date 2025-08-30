using System;
using System.Linq;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;

namespace PIA_Admin_Dashboard.Controllers
{
    public class LogsController : AuthenticatedController
    {
        public ActionResult LoginLogs(DateTime? fromDate, DateTime? toDate, string status, string searchValue)
        {
            using (var db = new ApplicationDbContext())
            {
                var query = from log in db.Login_Logs
                            join agent in db.Agents
                                on log.agent_uid equals agent.AgentUid
                            select new LogEntry
                            {
                                EmployeeId = agent.PNO,
                                EmployeeName = agent.Name,
                                IpAddress = log.ip_address,
                                Status = log.was_successful ? "Success" : "Failed",
                                DateTime = log.login_time
                            };

                // Search by Employee ID or Name
                if (!string.IsNullOrEmpty(searchValue))
                {
                    string val = searchValue.Trim().ToLower();
                    query = query.Where(l =>
                        (l.EmployeeId != null && l.EmployeeId.ToLower().Contains(val)) ||
                        (l.EmployeeName != null && l.EmployeeName.ToLower().Contains(val)));
                }

                // Date range filter
                if (fromDate.HasValue)
                    query = query.Where(l => l.DateTime >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(l => l.DateTime <= toDate.Value);

                // Status filter
                if (!string.IsNullOrEmpty(status) && status != "All")
                    query = query.Where(l => l.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

                // Order by latest first
                var model = query.OrderByDescending(l => l.DateTime).ToList();

                return View("~/Views/Admin/Logs/LoginLogs.cshtml", model);
            }
        }
    }
}
