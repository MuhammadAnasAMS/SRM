using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;

namespace PIA_Admin_Dashboard.Controllers
{
    public class ComplaintsController : Controller
    {
        // GET: Complaints
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ComplaintsLog(string searchBy, string searchValue, DateTime? fromDate, DateTime? toDate, string orderBy, string status)
        {
            var model = new ComplaintsLog();

            using (var db = new ApplicationDbContext())
            {
                var query = db.Request_Master.AsQueryable();

                // Search filter
                if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchValue))
                {
                    string val = searchValue.Trim().ToLower();

                    switch (searchBy)
                    {
                        case "RequestID":
                            if (int.TryParse(val, out int reqId))
                                query = query.Where(r => r.RequestID == reqId);
                            break;

                        case "Subject":
                            query = query.Where(r => r.ReqSummary != null && r.ReqSummary.ToLower().Contains(val.ToLower()));
                            break;

                        case "Site":
                            query = query.Where(r => r.Location != null && r.Location.ToLower().Contains(val.ToLower()));
                            break;

                        case "ReportedEmpID":
                            query = query.Where(r => r.Ownership != null && r.Ownership.ToLower().Contains(val.ToLower()));
                            break;

                        case "ForwardToEmpID":
                            query = query
                                .AsEnumerable()
                                .Where(r => r.ForwardTo != null &&
                                            r.ForwardTo.StartsWith("P") &&
                                            int.TryParse(r.ForwardTo.Substring(1), out _) &&
                                            r.ForwardTo.Equals(val, StringComparison.OrdinalIgnoreCase))
                                .AsQueryable();
                            break;

                        case "ForwardToGroupID":
                            query = query
                                .AsEnumerable()
                                .Where(r => r.ForwardTo != null &&
                                            r.ForwardTo.Length == 2 &&
                                            int.TryParse(r.ForwardTo, out _) &&
                                            r.ForwardTo.Equals(val, StringComparison.OrdinalIgnoreCase))
                                .AsQueryable();
                            break;
                    }
                }
                // Date filter
                if (fromDate.HasValue)
                    query = query.Where(r => r.RequestDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(r => r.RequestDate <= toDate.Value);

                // Status filter
                if (!string.IsNullOrEmpty(status) && status != "All")
                    query = query.Where(r => r.Status != null && r.Status == status);

                // Order
                query = orderBy == "RequestID" ? query.OrderByDescending(r => r.RequestID)
                                               : query.OrderByDescending(r => r.RequestDate);

                // Data projection
                model.Requests = query
                    .ToList()
                    .Select(r => new RequestItem
                    {
                        RequestID = r.RequestID,
                        Subject = r.ReqSummary,
                        RequestDate = r.RequestDate ?? DateTime.MinValue,
                        ForwardTo = r.ForwardTo,
                        Site = r.Location,
                        Status = r.Status,
                        PendingSince = r.Status == "C"
                          ? "-"
                         : r.ForwardedDate.HasValue
                          ? (DateTime.Now - r.ForwardedDate.Value).Days + " days"
                        : "N/A"
                    }).ToList();

                // Stats section
                model.Stats = new ComplaintStats
                {
                    Queue = db.Request_Master.Count(r => r.Status == "Q"),
                    Forwarded = db.Request_Master.Count(r => r.Status == "F"),
                    Resolved = db.Request_Master.Count(r => r.Status == "R"),
                    Closed = db.Request_Master.Count(r => r.Status == "C")
                };

                // ViewBag or model filters (optional for UI display)
                model.SearchBy = searchBy;
                model.SearchValue = searchValue;
                model.FromDate = fromDate;
                model.ToDate = toDate;
                model.OrderBy = orderBy;
                model.Status = status;
            }

            return View("~/Views/Admin/Complaints/ComplaintsLog.cshtml", model);
        }
        public ActionResult ViewLog(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                var request = db.Request_Master.FirstOrDefault(r => r.RequestID == id);
                if (request == null)
                {
                    return HttpNotFound();
                }

                return View("~/Views/Admin/Complaints/ViewLog.cshtml", request); // ViewLog.cshtml
            }
        }
    }
}