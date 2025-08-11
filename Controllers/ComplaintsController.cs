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

                // Fetch the Agent email using RequestFor (which is Pno)
                var agent = db.Agents.FirstOrDefault(a => a.PNO == request.RequestFor);
                request.Email = agent?.Email ?? "Email not found";
                ViewBag.Groups = db.Groups.ToList();
                ViewBag.Agents = db.Agents.ToList();

                return View("~/Views/Admin/Complaints/ViewLog.cshtml", request);
            }
        }

        [HttpPost]
        public ActionResult ForwardRequest(int RequestID, string ForwardType, string ForwardToAgent, int? ForwardToGroup)
        {
            using (var db = new ApplicationDbContext())
            {
                var request = db.Request_Master.FirstOrDefault(r => r.RequestID == RequestID);
                if (request == null) return HttpNotFound();

                string forwardedTo = "";
                string forwardedToDisplayName = "";
                string forwardTypeCode = ""; // I = Individual, G = Group

                if (ForwardType == "Group" && ForwardToGroup.HasValue)
                {
                    var group = db.Groups.FirstOrDefault(g => g.Gid == ForwardToGroup.Value);
                    forwardedTo = group?.Gid.ToString();       // store ID in DB
                    forwardedToDisplayName = group?.GroupName; // store name in log
                    forwardTypeCode = "G";
                }
                else if (ForwardType == "Individual" && !string.IsNullOrEmpty(ForwardToAgent))
                {
                    var agent = db.Agents.FirstOrDefault(a => a.PNO == ForwardToAgent);
                    forwardedTo = agent?.PNO;    // store PNO in DB
                    forwardedToDisplayName = agent?.Name; // store name in log
                    forwardTypeCode = "I";
                }

                string forwardedBy = User.Identity.Name;

                // Append to ReqDetails with display name
                string logEntry = $"<hr>{DateTime.Now:dd-MMM-yyyy}&nbsp;{DateTime.Now:hh:mm tt} forward to {forwardedToDisplayName} by {forwardedBy}";
                request.ReqDetails = (request.ReqDetails ?? "") + logEntry;

                // Update DB fields
                request.ForwardToType = forwardTypeCode;
                request.ForwardTo = forwardedTo;
                request.ForwardBy = forwardedBy;
                request.ForwardedDate = DateTime.Now;
                request.Status = "F";

                db.SaveChanges();

                return RedirectToAction("ComplaintsLog");
            }
        }
        [HttpPost]
        public ActionResult TakeOwnership(int requestId)
        {
            string currentUserPNO = User.Identity.Name;

            using (var db = new ApplicationDbContext())
            {
                var request = db.Request_Master.FirstOrDefault(r => r.RequestID == requestId);
                if (request != null)
                {
                    request.Ownership = currentUserPNO;
                    request.Status = "P";
                    db.SaveChanges();
                }
            }

            TempData["SuccessMessage"] = "You have taken ownership of the request.";
            return RedirectToAction("ComplaintsLog");
        }

        [HttpGet]
        public ActionResult FileComplaint()
        {

            return View("~/Views/Admin/Complaints/FileComplaint.cshtml", new ComplaintFormViewModel());
        }

            [HttpPost]
            public ActionResult FileComplaint(ComplaintFormViewModel model, string action)
            {
                using (var db = new ApplicationDbContext())
                {
                if (action == "fetch")
                {
                    model.Name = model.Email = model.Mobile = model.ProgramName = model.Location = null;

                    var agent = db.Agents.FirstOrDefault(a => a.PNO == model.pno);
                    if (agent != null)
                    {
                        model.Name = agent.Name;
                        model.Email = agent.Email;
                        model.Mobile = agent.Mobile;

                        var program = db.Programs.FirstOrDefault(p => p.ProgramId == agent.ProgramId);
                        model.ProgramName = program?.Name;

                        var firstWorkArea = (agent.WorkArea ?? "").Split(',').FirstOrDefault()?.Trim();

                        if (int.TryParse(firstWorkArea, out int workAreaId))
                        {
                            var location = db.Locations.FirstOrDefault(l => l.Sno == workAreaId);
                            model.Location = location?.LocationID;
                        }
                        else
                        {
                            model.Location = null;
                        }
                        model.RequestedIPAddress = agent.LastLoginIP;


                        model.IsDetailsFetched = true;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Employee ID not found.");
                        model.IsDetailsFetched = false;
                    }

                    // ⚠️ Clear old ModelState so new values appear in form fields
                    ModelState.Clear();

                    return View("~/Views/Admin/Complaints/FileComplaint.cshtml", model);
                }


                // Submit complaint
                if (action == "submit" && ModelState.IsValid)
                {
                    int priorityValue = 4; // Default to Normal

                    switch (model.Priority)
                    {
                        case "Critical":
                            priorityValue = 1;
                            break;
                        case "Urgent":
                            priorityValue = 2;
                            break;
                        case "Important":
                            priorityValue = 3;
                            break;
                        case "Normal":
                            priorityValue = 4;
                            break;
                    }

                    var request = new Request_Master
                    {
                        ReqSummary = model.ReqSummary,
                        ReqDetails = model.ReqDetails,
                        RequestDate = DateTime.Now,
                        RequestFor = model.pno,
                        RequestedIPAddress = model.RequestedIPAddress,
                        Location = model.Location,
                        Priority = priorityValue, // use integer value
                        PArea = string.Join(",", model.ProblemAreas),
                        Status = "Q"
                    };

                    db.Request_Master.Add(request);
                    db.SaveChanges();

                    TempData["Success"] = "Complaint submitted successfully.";
                        return RedirectToAction("ComplaintsLog");
                    }

                    return View("~/Views/Admin/Complaints/FileComplaint.cshtml", model);
                }
            }
    }
}