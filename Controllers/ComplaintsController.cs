using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;


namespace PIA_Admin_Dashboard.Controllers
{
    public class ComplaintsController : AuthenticatedController
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

                var agent = db.Agents.FirstOrDefault(a => a.PNO == request.RequestFor);
                request.Email = agent?.Email ?? "Email not found";

                ViewBag.Agents = db.Agents.ToList();
                ViewBag.Groups = db.Groups.ToList();

                // FIXED: Get current logged-in user's PNO correctly
                string currentUserPNO = null;
                if (Session["AgentUid"] != null)
                {
                    var uid = Session["AgentUid"].ToString();
                    var currentAgent = db.Agents.FirstOrDefault(a => a.AgentUid == uid);
                    currentUserPNO = currentAgent?.PNO;
                }

                // FIXED: Check if ownership form should be shown
                // Show form if: user owns the request AND status is not closed AND TempData indicates ownership was just taken
                ViewBag.ShowAdditionalForm =
                    (TempData["ShowOwnershipDetails"] != null && (bool)TempData["ShowOwnershipDetails"]) ||
                    (request.Ownership == currentUserPNO &&
                     !string.Equals(request.Status, "C", StringComparison.OrdinalIgnoreCase));

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
                string forwardedByName = null;
                string forwardedBy = null;
                if (Session["AgentUid"] != null)
                {
                    var uid = Session["AgentUid"].ToString();
                    var agent = db.Agents.FirstOrDefault(a => a.AgentUid == uid);
                    forwardedByName = agent?.Name;
                    forwardedBy = agent?.PNO;
                }
                request.ForwardBy = forwardedBy;
                // Append to ReqDetails with display name
                string logEntry = $"<hr>{DateTime.Now:dd-MMM-yyyy}&nbsp;{DateTime.Now:hh:mm tt} forward to {forwardedToDisplayName} by {forwardedByName}";
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
            using (var db = new ApplicationDbContext())
            {
                // Get current user info
                string currentUserPNO = null;
                string currentUserName = "Unknown User";

                if (Session["AgentUid"] != null)
                {
                    var uid = Session["AgentUid"].ToString();
                    var agent = db.Agents.FirstOrDefault(a => a.AgentUid == uid);
                    if (agent != null)
                    {
                        currentUserPNO = agent.PNO;
                        currentUserName = agent.Name;
                    }
                }

                // Find the request
                var request = db.Request_Master.FirstOrDefault(r => r.RequestID == requestId);
                if (request != null)
                {
                    // Update ownership and status
                    request.Ownership = currentUserPNO;
                    request.Status = "P"; // In Progress

                    // Add ownership log entry to ReqDetails
                    string timestamp = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
                    string ownershipLogEntry = $"<hr><strong>{timestamp}</strong> - Ownership taken by <strong>{currentUserName}</strong><br/>";

                    // Append to existing details (preserves all previous data)
                    request.ReqDetails = (request.ReqDetails ?? "") + ownershipLogEntry;

                    // Save changes to database
                    db.SaveChanges();
                }

                // Set TempData to show the additional form
                TempData["ShowOwnershipDetails"] = true;
                TempData["SuccessMessage"] = $"Ownership successfully taken by {currentUserName}";

                return RedirectToAction("ViewLog", new { id = requestId });
            }
        }




        public ActionResult SaveOwnershipDetails(int requestId, string ActualPArea, string AdditionalDetails, string Status)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var request = db.Request_Master.FirstOrDefault(r => r.RequestID == requestId);
                    if (request != null)
                    {
                        // Get current user info for logging
                        string currentUserName = "Unknown";
                        string currentUserPNO = null;

                        if (Session["AgentUid"] != null)
                        {
                            var uid = Session["AgentUid"].ToString();
                            var agent = db.Agents.FirstOrDefault(a => a.AgentUid == uid);
                            if (agent != null)
                            {
                                currentUserName = agent.Name;
                                currentUserPNO = agent.PNO;
                            }
                        }

                        // Update the actual problem area (store abbreviation)
                        request.PArea = ActualPArea;

                        // Map abbreviation to full name for logging
                        var areaMapping = new Dictionary<string, string>
                        {
                            {"HW", "Hardware"},
                            {"SW", "Software"},
                            {"INT", "Internet"},
                            {"NW", "Network"},
                            {"O", "Other"}
                        };

                        string fullAreaName = areaMapping.ContainsKey(ActualPArea) ? areaMapping[ActualPArea] : ActualPArea;

                        // Append additional details with proper formatting and timestamp
                        string timestamp = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
                        string logEntry = $"<hr><strong>{timestamp}</strong> - Ownership Details updated by <strong>{currentUserName}</strong><br/>" +
                                        $"<strong>Additional Details:</strong> {AdditionalDetails}<br/>";

                        // Append to existing details (preserves all previous data)
                        request.ReqDetails = (request.ReqDetails ?? "") + logEntry;

                        // Update status
                        request.Status = Status;

                        // Save changes to database
                        db.SaveChanges();

                        TempData["SuccessMessage"] = "Ownership details saved successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Request not found.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving details. Please try again.";
                System.Diagnostics.Debug.WriteLine($"Error in SaveOwnershipDetails: {ex.Message}");
            }

            return RedirectToAction("ViewLog", new { id = requestId });
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