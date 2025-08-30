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
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the complaints page.";
                System.Diagnostics.Debug.WriteLine($"Error in Complaints Index: {ex.Message}");
                return View("Error");
            }
        }

        public ActionResult ComplaintsLog(string searchBy, string searchValue, DateTime? fromDate, DateTime? toDate, string orderBy, string status)
        {
            try
            {
                var model = new ComplaintsLog();

                using (var db = new ApplicationDbContext())
                {
                    var query = db.Request_Master.AsQueryable();

                    // Search filter with exception handling
                    if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchValue))
                    {
                        try
                        {
                            string val = searchValue.Trim().ToLower();

                            switch (searchBy)
                            {
                                case "RequestID":
                                    if (int.TryParse(val, out int reqId))
                                        query = query.Where(r => r.RequestID == reqId);
                                    break;

                                case "Subject":
                                    query = query.Where(r => r.ReqSummary != null && r.ReqSummary.ToLower().Contains(val));
                                    break;

                                case "Site":
                                    query = query.Where(r => r.Location != null && r.Location.ToLower().Contains(val));
                                    break;

                                case "ReportedEmpID":
                                    query = query.Where(r => r.Ownership != null && r.Ownership.ToLower().Contains(val));
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
                        catch (Exception searchEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in search filter: {searchEx.Message}");
                            TempData["WarningMessage"] = "Search filter encountered an issue. Showing all results.";
                        }
                    }

                    // Date filter with exception handling
                    try
                    {
                        if (fromDate.HasValue)
                            query = query.Where(r => r.RequestDate >= fromDate.Value);

                        if (toDate.HasValue)
                            query = query.Where(r => r.RequestDate <= toDate.Value);
                    }
                    catch (Exception dateEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in date filter: {dateEx.Message}");
                        TempData["WarningMessage"] = "Date filter encountered an issue. Showing all results.";
                    }

                    // Status filter with exception handling
                    try
                    {
                        if (!string.IsNullOrEmpty(status) && status != "All")
                            query = query.Where(r => r.Status != null && r.Status == status);
                    }
                    catch (Exception statusEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in status filter: {statusEx.Message}");
                        TempData["WarningMessage"] = "Status filter encountered an issue. Showing all results.";
                    }

                    // Order with exception handling
                    try
                    {
                        query = orderBy == "RequestID" ? query.OrderByDescending(r => r.RequestID)
                                                       : query.OrderByDescending(r => r.RequestDate);
                    }
                    catch (Exception orderEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in ordering: {orderEx.Message}");
                        query = query.OrderByDescending(r => r.RequestID); // fallback ordering
                    }

                    // Data projection with exception handling
                    try
                    {
                        model.Requests = query
                            .ToList()
                            .Select(r => new RequestItem
                            {
                                RequestID = r.RequestID,
                                Subject = r.ReqSummary ?? "No Subject",
                                RequestDate = r.RequestDate ?? DateTime.MinValue,
                                ForwardTo = r.ForwardTo ?? "",
                                Site = r.Location ?? "",
                                Status = r.Status ?? "",
                                PendingSince = r.Status == "C"
                                  ? "-"
                                 : r.ForwardedDate.HasValue
                                  ? (DateTime.Now - r.ForwardedDate.Value).Days + " days"
                                : "N/A"
                            }).ToList();
                    }
                    catch (Exception projectionEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in data projection: {projectionEx.Message}");
                        model.Requests = new List<RequestItem>(); // empty list as fallback
                        TempData["ErrorMessage"] = "Error loading complaint data. Please try again.";
                    }

                    // Set filter values for UI
                    model.SearchBy = searchBy;
                    model.SearchValue = searchValue;
                    model.FromDate = fromDate;
                    model.ToDate = toDate;
                    model.OrderBy = orderBy;
                    model.Status = status;
                }

                return View("~/Views/Admin/Complaints/ComplaintsLog.cshtml", model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ComplaintsLog: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the complaints log.";
                return View("~/Views/Admin/Complaints/ComplaintsLog.cshtml", new ComplaintsLog());
            }
        }
        public ActionResult ViewLog(int id)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var request = db.Request_Master.FirstOrDefault(r => r.RequestID == id);
                    if (request == null)
                    {
                        TempData["ErrorMessage"] = "Request not found.";
                        return RedirectToAction("ComplaintsLog");
                    }

                    try
                    {
                        var agent = db.Agents.FirstOrDefault(a => a.PNO == request.RequestFor);
                        request.Email = agent?.Email ?? "Email not found";

                        ViewBag.Agents = db.Agents.ToList();
                        ViewBag.Groups = db.Groups.ToList();
                    }
                    catch (Exception agentEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading agent/group data: {agentEx.Message}");
                        request.Email = "Error loading email";
                        ViewBag.Agents = new List<object>();
                        ViewBag.Groups = new List<object>();
                        TempData["WarningMessage"] = "Some data could not be loaded completely.";
                    }

                    try
                    {
                        // Get current logged-in user's PNO correctly
                        string currentUserPNO = null;
                        if (Session["AgentUid"] != null)
                        {
                            var uid = Session["AgentUid"].ToString();
                            var currentAgent = db.Agents.FirstOrDefault(a => a.AgentUid == uid);
                            currentUserPNO = currentAgent?.PNO;
                        }

                        // Check if ownership form should be shown
                        ViewBag.ShowAdditionalForm =
                            (TempData["ShowOwnershipDetails"] != null && (bool)TempData["ShowOwnershipDetails"]) ||
                            (request.Ownership == currentUserPNO &&
                             !string.Equals(request.Status, "C", StringComparison.OrdinalIgnoreCase));
                    }
                    catch (Exception userEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error checking user permissions: {userEx.Message}");
                        ViewBag.ShowAdditionalForm = false;
                    }

                    return View("~/Views/Admin/Complaints/ViewLog.cshtml", request);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ViewLog: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the request details.";
                return RedirectToAction("ComplaintsLog");
            }
        }


        [HttpPost]
        public ActionResult ForwardRequest(int RequestID, string ForwardType, string ForwardToAgent, int? ForwardToGroup)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var request = db.Request_Master.FirstOrDefault(r => r.RequestID == RequestID);
                    if (request == null)
                    {
                        TempData["ErrorMessage"] = "Request not found.";
                        return RedirectToAction("ComplaintsLog");
                    }

                    // Check if request is in a state that allows forwarding
                    if (request.Status == "C")
                    {
                        TempData["ErrorMessage"] = "Cannot forward a closed request.";
                        return RedirectToAction("ViewLog", new { id = RequestID });
                    }

                    string forwardedTo = "";
                    string forwardedToDisplayName = "";
                    string forwardTypeCode = ""; // I = Individual, G = Group

                    try
                    {
                        if (ForwardType == "Group" && ForwardToGroup.HasValue)
                        {
                            var group = db.Groups.FirstOrDefault(g => g.Gid == ForwardToGroup.Value);
                            if (group != null)
                            {
                                forwardedTo = group.Gid.ToString();       // store ID in DB
                                forwardedToDisplayName = group.GroupName; // store name in log
                                forwardTypeCode = "G";
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Selected group not found.";
                                return RedirectToAction("ViewLog", new { id = RequestID });
                            }
                        }
                        else if (ForwardType == "Individual" && !string.IsNullOrEmpty(ForwardToAgent))
                        {
                            var agent = db.Agents.FirstOrDefault(a => a.PNO == ForwardToAgent);
                            if (agent != null)
                            {
                                forwardedTo = agent.PNO;    // store PNO in DB
                                forwardedToDisplayName = agent.Name; // store name in log
                                forwardTypeCode = "I";
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Selected agent not found.";
                                return RedirectToAction("ViewLog", new { id = RequestID });
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Please select a valid forward option.";
                            return RedirectToAction("ViewLog", new { id = RequestID });
                        }
                    }
                    catch (Exception forwardEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing forward target: {forwardEx.Message}");
                        TempData["ErrorMessage"] = "Error processing forward target. Please try again.";
                        return RedirectToAction("ViewLog", new { id = RequestID });
                    }

                    try
                    {
                        string forwardedByName = "Unknown";
                        string forwardedBy = null;
                        if (Session["AgentUid"] != null)
                        {
                            var uid = Session["AgentUid"].ToString();
                            var agent = db.Agents.FirstOrDefault(a => a.AgentUid == uid);
                            if (agent != null)
                            {
                                forwardedByName = agent.Name;
                                forwardedBy = agent.PNO;
                            }
                        }

                        if (string.IsNullOrEmpty(forwardedBy))
                        {
                            TempData["ErrorMessage"] = "Unable to identify current user. Please log in again.";
                            return RedirectToAction("ViewLog", new { id = RequestID });
                        }

                        // Update request fields
                        request.ForwardBy = forwardedBy;

                        // Append to ReqDetails with display name
                        string logEntry = $"<hr>{DateTime.Now:dd-MMM-yyyy}&nbsp;{DateTime.Now:hh:mm tt} forward to {forwardedToDisplayName} by {forwardedByName}";

                        try
                        {
                            request.ReqDetails = (request.ReqDetails ?? "") + logEntry;
                        }
                        catch (Exception logEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error updating request details: {logEx.Message}");
                            TempData["WarningMessage"] = "Request forwarded but logging encountered an issue.";
                        }

                        // Update DB fields
                        request.ForwardToType = forwardTypeCode;
                        request.ForwardTo = forwardedTo;
                        request.ForwardedDate = DateTime.Now;
                        request.Status = "F";

                        db.SaveChanges();

                        TempData["SuccessMessage"] = $"Request forwarded to {forwardedToDisplayName} successfully.";
                        return RedirectToAction("ComplaintsLog");
                    }
                    catch (Exception updateEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating request: {updateEx.Message}");
                        TempData["ErrorMessage"] = "Error forwarding request. Please try again.";
                        return RedirectToAction("ViewLog", new { id = RequestID });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ForwardRequest: {ex.Message}");
                TempData["ErrorMessage"] = "An unexpected error occurred while forwarding the request.";
                return RedirectToAction("ComplaintsLog");
            }
        }
        [HttpPost]
        public ActionResult TakeOwnership(int requestId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    // Find the request first
                    var request = db.Request_Master.FirstOrDefault(r => r.RequestID == requestId);
                    if (request == null)
                    {
                        TempData["ErrorMessage"] = "Request not found.";
                        return RedirectToAction("ComplaintsLog");
                    }

                    // Check if request is in a valid state for taking ownership
                    if (request.Status == "C")
                    {
                        TempData["ErrorMessage"] = "Cannot take ownership of a closed request.";
                        return RedirectToAction("ViewLog", new { id = requestId });
                    }

                    // Get current user info
                    string currentUserPNO = null;
                    string currentUserName = "Unknown User";

                    try
                    {
                        if (Session != null && Session["AgentUid"] != null)
                        {
                            var uid = Session["AgentUid"].ToString();
                            if (!string.IsNullOrEmpty(uid))
                            {
                                var agent = db.Agents.FirstOrDefault(a => a.AgentUid == uid);
                                if (agent != null)
                                {
                                    currentUserPNO = agent.PNO;
                                    currentUserName = agent.Name ?? "Unknown User";
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(currentUserPNO))
                        {
                            TempData["ErrorMessage"] = "Unable to identify current user. Please log in again.";
                            return RedirectToAction("ViewLog", new { id = requestId });
                        }
                    }
                    catch (Exception userEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting current user: {userEx.Message}");
                        TempData["ErrorMessage"] = "Error identifying current user. Please try again.";
                        return RedirectToAction("ViewLog", new { id = requestId });
                    }

                    try
                    {
                        // Check if request is already owned by someone else
                        if (!string.IsNullOrEmpty(request.Ownership) && request.Ownership != currentUserPNO)
                        {
                            try
                            {
                                var ownerAgent = db.Agents.FirstOrDefault(a => a.PNO == request.Ownership);
                                string ownerName = ownerAgent?.Name ?? "Another user";
                                TempData["ErrorMessage"] = $"This request is already owned by {ownerName}.";
                            }
                            catch (Exception ownerEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error getting owner info: {ownerEx.Message}");
                                TempData["ErrorMessage"] = "This request is already owned by another user.";
                            }
                            return RedirectToAction("ViewLog", new { id = requestId });
                        }

                        // Update ownership and status
                        request.Ownership = currentUserPNO;
                        request.Status = "P"; // In Progress

                        // Add ownership log entry to ReqDetails
                        string timestamp = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
                        string ownershipLogEntry = $"<hr><strong>{timestamp}</strong> - Ownership taken by <strong>{currentUserName}</strong><br/>";

                        // Safely append to existing details
                        try
                        {
                            request.ReqDetails = (request.ReqDetails ?? "") + ownershipLogEntry;
                        }
                        catch (Exception logEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error updating request details: {logEx.Message}");
                            TempData["WarningMessage"] = "Ownership taken but logging encountered an issue.";
                        }

                        // Save changes to database
                        db.SaveChanges();

                        // Set TempData to show the additional form
                        TempData["ShowOwnershipDetails"] = true;
                        TempData["SuccessMessage"] = $"Ownership successfully taken by {currentUserName}";
                    }
                    catch (Exception updateEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating ownership: {updateEx.Message}");
                        TempData["ErrorMessage"] = "Error taking ownership. Please try again.";
                    }

                    return RedirectToAction("ViewLog", new { id = requestId });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TakeOwnership: {ex.Message}");
                TempData["ErrorMessage"] = "An unexpected error occurred while taking ownership.";
                return RedirectToAction("ComplaintsLog");
            }
        }

        // UPDATED SAVEOWNERSHIPDETAILS METHOD
        public ActionResult SaveOwnershipDetails(int requestId, string ActualPArea, string AdditionalDetails, string Status)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var request = db.Request_Master.FirstOrDefault(r => r.RequestID == requestId);
                    if (request == null)
                    {
                        TempData["ErrorMessage"] = "Request not found.";
                        return RedirectToAction("ComplaintsLog");
                    }

                    // Get current user info for logging
                    string currentUserName = "Unknown";
                    string currentUserPNO = null;

                    try
                    {
                        if (Session != null && Session["AgentUid"] != null)
                        {
                            var uid = Session["AgentUid"].ToString();
                            if (!string.IsNullOrEmpty(uid))
                            {
                                var agent = db.Agents.FirstOrDefault(a => a.AgentUid == uid);
                                if (agent != null)
                                {
                                    currentUserName = agent.Name ?? "Unknown";
                                    currentUserPNO = agent.PNO;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(currentUserPNO))
                        {
                            TempData["ErrorMessage"] = "Unable to identify current user. Please log in again.";
                            return RedirectToAction("ViewLog", new { id = requestId });
                        }
                    }
                    catch (Exception userEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting current user in SaveOwnershipDetails: {userEx.Message}");
                        TempData["ErrorMessage"] = "Error identifying current user. Please try again.";
                        return RedirectToAction("ViewLog", new { id = requestId });
                    }

                    try
                    {
                        // Validate inputs
                        if (string.IsNullOrEmpty(Status))
                        {
                            TempData["ErrorMessage"] = "Status is required.";
                            return RedirectToAction("ViewLog", new { id = requestId });
                        }

                        // Validate status transitions
                        var validTransitions = new Dictionary<string, List<string>>
                {
                    { "Q", new List<string> { "P", "F", "C" } }, // Queue can go to Progress, Forward, or Close
                    { "P", new List<string> { "R", "F", "C" } }, // Progress can go to Resolved, Forward, or Close
                    { "F", new List<string> { "P", "R", "C" } }, // Forwarded can go to Progress, Resolved, or Close
                    { "R", new List<string> { "C", "P" } },      // Resolved can go to Close or back to Progress
                    { "C", new List<string> { } }                // Closed is final state
                };

                        if (!string.IsNullOrEmpty(request.Status) &&
                            validTransitions.ContainsKey(request.Status) &&
                            !validTransitions[request.Status].Contains(Status))
                        {
                            TempData["ErrorMessage"] = $"Invalid status transition from {request.Status} to {Status}.";
                            return RedirectToAction("ViewLog", new { id = requestId });
                        }

                        // Update the actual problem area (store abbreviation)
                        if (!string.IsNullOrEmpty(ActualPArea))
                        {
                            request.PArea = ActualPArea;
                        }

                        // Get current timestamp
                        DateTime currentDateTime = DateTime.Now;
                        string timestamp = currentDateTime.ToString("dd-MMM-yyyy hh:mm tt");

                        // Handle status-specific updates
                        string previousStatus = request.Status;

                        // RESOLVE TRACKING: Check if status is changing to Resolved (R)
                        if (Status == "R" && previousStatus != "R")
                        {
                            try
                            {
                                request.ReqResolveDate = currentDateTime;
                                request.ReqResolveBy = currentUserPNO;

                                // Add resolved log entry
                                string resolvedLogEntry = $"<hr><strong>{timestamp}</strong> - Request <strong>RESOLVED</strong> by <strong>{currentUserName}</strong><br/>";

                                try
                                {
                                    request.ReqDetails = (request.ReqDetails ?? "") + resolvedLogEntry;
                                }
                                catch (Exception logEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error adding resolve log: {logEx.Message}");
                                    TempData["WarningMessage"] = "Request resolved but logging encountered an issue.";
                                }
                            }
                            catch (Exception resolveEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error updating resolve information: {resolveEx.Message}");
                                TempData["WarningMessage"] = "Request status updated but resolve tracking encountered an issue.";
                            }
                        }
                        // CLOSE TRACKING: Check if status is changing to Closed (C)
                        else if (Status == "C" && previousStatus != "C")
                        {
                            try
                            {
                                request.ReqCloseDate = currentDateTime;
                                request.ReqCloseBy = currentUserPNO;

                                // Add closed log entry
                                string closedLogEntry = $"<hr><strong>{timestamp}</strong> - Request <strong>CLOSED</strong> by <strong>{currentUserName}</strong><br/>";

                                try
                                {
                                    request.ReqDetails = (request.ReqDetails ?? "") + closedLogEntry;
                                }
                                catch (Exception logEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error adding close log: {logEx.Message}");
                                    TempData["WarningMessage"] = "Request closed but logging encountered an issue.";
                                }
                            }
                            catch (Exception closeEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error updating close information: {closeEx.Message}");
                                TempData["WarningMessage"] = "Request status updated but close tracking encountered an issue.";
                            }
                        }

                        // Append additional details with proper formatting and timestamp
                        if (!string.IsNullOrEmpty(AdditionalDetails))
                        {
                            try
                            {
                                string logEntry = $"<hr><strong>{timestamp}</strong> - Ownership Details updated by <strong>{currentUserName}</strong><br/>" +
                                                $"<strong>Additional Details:</strong> {AdditionalDetails}<br/>";

                                // Append to existing details (preserves all previous data)
                                request.ReqDetails = (request.ReqDetails ?? "") + logEntry;
                            }
                            catch (Exception detailsEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error adding additional details: {detailsEx.Message}");
                                TempData["WarningMessage"] = "Main updates saved but additional details logging encountered an issue.";
                            }
                        }

                        // Update status (this should be done after the status-specific checks)
                        request.Status = Status;

                        // Save changes to database
                        db.SaveChanges();

                        // Set success message based on status
                        string successMessage = "Ownership details saved successfully!";
                        if (Status == "R")
                        {
                            successMessage = "Request resolved successfully!";
                        }
                        else if (Status == "C")
                        {
                            successMessage = "Request closed successfully!";
                        }

                        TempData["SuccessMessage"] = successMessage;
                    }
                    catch (Exception updateEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating request details: {updateEx.Message}");
                        TempData["ErrorMessage"] = "Error saving request details. Please try again.";
                        return RedirectToAction("ViewLog", new { id = requestId });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SaveOwnershipDetails: {ex.Message}");
                TempData["ErrorMessage"] = "An unexpected error occurred while saving details. Please try again.";
            }

            return RedirectToAction("ViewLog", new { id = requestId });
        }

        // UPDATED FILECOMPLAINT GET METHOD
        [HttpGet]
        public ActionResult FileComplaint()
        {
            try
            {
                return View("~/Views/Admin/Complaints/FileComplaint.cshtml", new ComplaintFormViewModel());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FileComplaint GET: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the complaint form.";
                return RedirectToAction("ComplaintsLog");
            }
        }

        // UPDATED FILECOMPLAINT POST METHOD
        [HttpPost]
        public ActionResult FileComplaint(ComplaintFormViewModel model, string action)
        {
            try
            {
                // Validate model is not null
                if (model == null)
                {
                    model = new ComplaintFormViewModel();
                }

                using (var db = new ApplicationDbContext())
                {
                    if (action == "fetch")
                    {
                        try
                        {
                            // Validate PNO input
                            if (string.IsNullOrEmpty(model.pno))
                            {
                                ModelState.AddModelError("pno", "Employee ID is required.");
                                return View("~/Views/Admin/Complaints/FileComplaint.cshtml", model);
                            }

                            // Reset previous data
                            model.Name = model.Email = model.Mobile = model.ProgramName = model.Location = null;
                            model.IsDetailsFetched = false;

                            var agent = db.Agents.FirstOrDefault(a => a.PNO == model.pno);
                            if (agent != null)
                            {
                                try
                                {
                                    model.Name = agent.Name ?? "Unknown Name";
                                    model.Email = agent.Email ?? "No Email";
                                    model.Mobile = agent.Mobile ?? "No Mobile";

                                    // Get program details with error handling
                                    try
                                    {
                                        var program = db.Programs.FirstOrDefault(p => p.ProgramId == agent.ProgramId);
                                        model.ProgramName = program?.Name ?? "Unknown Program";
                                    }
                                    catch (Exception programEx)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Error fetching program: {programEx.Message}");
                                        model.ProgramName = "Error loading program";
                                    }

                                    // Get work area details with error handling
                                    try
                                    {
                                        var firstWorkArea = (agent.WorkArea ?? "").Split(',').FirstOrDefault()?.Trim();

                                        if (int.TryParse(firstWorkArea, out int workAreaId))
                                        {
                                            var location = db.Locations.FirstOrDefault(l => l.Sno == workAreaId);
                                            model.Location = location?.LocationID ?? "Unknown Location";
                                        }
                                        else
                                        {
                                            model.Location = "Unknown Location";
                                        }
                                    }
                                    catch (Exception locationEx)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Error fetching location: {locationEx.Message}");
                                        model.Location = "Error loading location";
                                    }

                                    model.RequestedIPAddress = agent.LastLoginIP ?? "0.0.0.0";
                                    model.IsDetailsFetched = true;

                                    TempData["InfoMessage"] = "Employee details fetched successfully.";
                                }
                                catch (Exception detailsEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error fetching agent details: {detailsEx.Message}");
                                    ModelState.AddModelError("", "Error loading employee details. Some information may be incomplete.");
                                    model.IsDetailsFetched = true; // still allow form to show
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("pno", "Employee ID not found.");
                                model.IsDetailsFetched = false;
                            }

                            // Clear old ModelState so new values appear in form fields
                            ModelState.Clear();

                            return View("~/Views/Admin/Complaints/FileComplaint.cshtml", model);
                        }
                        catch (Exception fetchEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in fetch action: {fetchEx.Message}");
                            ModelState.AddModelError("", "Error fetching employee details. Please try again.");
                            return View("~/Views/Admin/Complaints/FileComplaint.cshtml", model);
                        }
                    }

                    // Submit complaint
                    if (action == "submit")
                    {
                        try
                        {
                            // Validate required fields
                            if (string.IsNullOrEmpty(model.pno))
                            {
                                ModelState.AddModelError("pno", "Employee ID is required.");
                            }
                            if (string.IsNullOrEmpty(model.ReqSummary))
                            {
                                ModelState.AddModelError("ReqSummary", "Subject is required.");
                            }
                            if (string.IsNullOrEmpty(model.ReqDetails))
                            {
                                ModelState.AddModelError("ReqDetails", "Complaint details are required.");
                            }
                            if (model.ProblemAreas == null || !model.ProblemAreas.Any())
                            {
                                ModelState.AddModelError("ProblemAreas", "At least one problem area must be selected.");
                            }
                            if (string.IsNullOrEmpty(model.Priority))
                            {
                                ModelState.AddModelError("Priority", "Priority is required.");
                            }

                            if (!ModelState.IsValid)
                            {
                                return View("~/Views/Admin/Complaints/FileComplaint.cshtml", model);
                            }

                            // Validate employee exists
                            var agentExists = db.Agents.Any(a => a.PNO == model.pno);
                            if (!agentExists)
                            {
                                ModelState.AddModelError("pno", "Employee ID not found in system.");
                                return View("~/Views/Admin/Complaints/FileComplaint.cshtml", model);
                            }

                            // Convert priority to integer value with error handling
                            int priorityValue = 4; // Default to Normal

                            try
                            {
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
                                    default:
                                        priorityValue = 4; // fallback to Normal
                                        break;
                                }
                            }
                            catch (Exception priorityEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error processing priority: {priorityEx.Message}");
                                priorityValue = 4; // fallback to Normal
                                TempData["WarningMessage"] = "Priority set to Normal due to processing error.";
                            }

                            // Get current user info for logging who filed the complaint
                            string filedBy = null;
                            string filedByName = "System";

                            try
                            {
                                if (Session != null && Session["AgentUid"] != null)
                                {
                                    var uid = Session["AgentUid"].ToString();
                                    if (!string.IsNullOrEmpty(uid))
                                    {
                                        var currentAgent = db.Agents.FirstOrDefault(a => a.AgentUid == uid);
                                        if (currentAgent != null)
                                        {
                                            filedBy = currentAgent.PNO;
                                            filedByName = currentAgent.Name ?? "Unknown Admin";
                                        }
                                    }
                                }
                            }
                            catch (Exception userEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error getting current user for filing: {userEx.Message}");
                                // Continue with system default
                            }

                            // Create initial complaint details with filing information
                            string initialDetails = model.ReqDetails?.Trim() ?? "";
                            string timestamp = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
                            string filingLogEntry = $"<strong>{timestamp}</strong> - Complaint filed by <strong>{filedByName}</strong> for employee <strong>{model.pno}</strong><br/><hr>";

                            string completeDetails = filingLogEntry + initialDetails;

                            // Create new request
                            var request = new Request_Master
                            {
                                ReqSummary = model.ReqSummary?.Trim() ?? "",
                                ReqDetails = completeDetails,
                                RequestDate = DateTime.Now,
                                RequestFor = model.pno, // The employee the complaint is filed for
                                RequestedIPAddress = model.RequestedIPAddress ?? "0.0.0.0",
                                Location = model.Location ?? "",
                                Priority = priorityValue,
                                PArea = model.ProblemAreas != null ? string.Join(",", model.ProblemAreas) : "",
                                Status = "Q", // Queue status
                                RequestLogBy = filedBy // Track who filed the complaint
                            };

                            db.Request_Master.Add(request);
                            db.SaveChanges();

                            // Log system event
                            try
                            {
                                System.Diagnostics.Debug.WriteLine($"AUDIT LOG: Complaint filed - ID: {request.RequestID}, For: {model.pno}, By: {filedByName}, Subject: {model.ReqSummary}");
                            }
                            catch (Exception logEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error in audit logging: {logEx.Message}");
                            }

                            TempData["SuccessMessage"] = $"Complaint submitted successfully. Request ID: {request.RequestID}";
                            return RedirectToAction("ComplaintsLog");
                        }
                        catch (Exception submitEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error submitting complaint: {submitEx.Message}");
                            TempData["ErrorMessage"] = "Error submitting complaint. Please try again.";
                            return View("~/Views/Admin/Complaints/FileComplaint.cshtml", model);
                        }
                    }

                    return View("~/Views/Admin/Complaints/FileComplaint.cshtml", model);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FileComplaint POST: {ex.Message}");
                TempData["ErrorMessage"] = "An unexpected error occurred while processing the complaint form.";
                return View("~/Views/Admin/Complaints/FileComplaint.cshtml", model ?? new ComplaintFormViewModel());
            }
        }
    }
}