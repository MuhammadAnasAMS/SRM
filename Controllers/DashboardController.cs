using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;

namespace PIA_Admin_Dashboard.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index(string searchBy, string searchValue, DateTime? fromDate, DateTime? toDate, string orderBy, string status)
        {
            var model = new DashboardViewModel();
            using (var db = new ApplicationDbContext())
            {
                var query = db.Request_Master.AsQueryable();

                // Filtering
                if (!string.IsNullOrEmpty(searchValue))
                {
                    switch (searchBy)
                    {
                        case "Subject":
                            query = query.Where(r => r.ReqSummary.Contains(searchValue));
                            break;
                        case "RequestID":
                            if (int.TryParse(searchValue, out int id))
                                query = query.Where(r => r.RequestID == id);
                            break;
                    }
                }

                if (fromDate.HasValue)
                    query = query.Where(r => r.RequestDate >= fromDate.Value);
                if (toDate.HasValue)
                    query = query.Where(r => r.RequestDate <= toDate.Value);

                if (!string.IsNullOrEmpty(status) && status != "All")
                    query = query.Where(r => r.Status == status);

                // Ordering
                if (orderBy == "RequestID")
                {
                    query = query.OrderByDescending(r => r.RequestID);
                }
                else if (orderBy == "RequestDate")
                {
                    query = query.OrderByDescending(r => r.RequestDate);
                }
                else
                {
                    query = query.OrderByDescending(r => r.RequestDate);
                }

                model.Requests = query.Select(r => new RequestItem
                {
                    RequestID = r.RequestID,
                    Subject = r.ReqSummary,
                    RequestDate = r.RequestDate ?? DateTime.MinValue,
                    ForwardTo = r.ForwardTo,
                    Site = r.Location,
                    Status = r.Status,
                    PendingSince = r.ForwardedDate.HasValue
                        ? (DateTime.Now - r.ForwardedDate.Value).Days + " days"
                        : "N/A"
                }).ToList();

                // Stats
                model.Stats = new DashboardStats
                {
                    Queue = db.Request_Master.Count(r => r.Status == "Q"),
                    Forwarded = db.Request_Master.Count(r => r.Status == "F"),
                    Resolved = db.Request_Master.Count(r => r.Status == "R"),
                    Closed = db.Request_Master.Count(r => r.Status == "C")
                };

                model.SearchBy = searchBy;
                model.SearchValue = searchValue;
                model.FromDate = fromDate;
                model.ToDate = toDate;
                model.OrderBy = orderBy;
                model.Status = status;
            }

            return View(model);
        }

    }
}