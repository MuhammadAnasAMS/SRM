using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;
using System.Data.Entity;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace PIA_Admin_Dashboard.Controllers
{
    public class AgentController : AuthenticatedController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var agents = db.Agents.ToList();
            // Load program ID to Name mapping
            var programMap = db.Programs
                .ToDictionary(p => p.ProgramId, p => p.Name); // Use actual column names

            ViewBag.ProgramMap = programMap;
            return View("~/Views/Admin/Agent/Index.cshtml", agents);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var agent = db.Agents.Find(id);
            if (agent == null)
                return HttpNotFound();

            return View("~/Views/Admin/Agent/Details.cshtml", agent);
        }

        public ActionResult Create()
        {
            var viewModel = new AgentViewModel
            {
                Agent = new Agent(),
                Roles = db.Roles.Select(r => new SelectListItem
                {
                    Value = r.RoleID.ToString(),
                    Text = r.Role_Name
                }).ToList(), // ✅ Fix here

                Locations = db.Locations.Select(l => new SelectListItem
                {
                    Value = l.Sno.ToString(),
                    Text = l.LocationDescription
                }).ToList(), // ✅ Fix here

                RoleSelectionType = "No Role"
            };

            return View("~/Views/Admin/Agent/Create.cshtml", viewModel); // ✅ Pass ViewModel to View
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AgentViewModel viewModel, string[] Agent_WorkArea)
        {
            if (ModelState.IsValid)
            {
                viewModel.Agent.LastUpdate = DateTime.Now;

                // Convert multi-select WorkArea to comma-separated string
                if (Agent_WorkArea != null && Agent_WorkArea.Any())
                {
                    viewModel.Agent.WorkArea = string.Join(",", Agent_WorkArea);
                }

                db.Agents.Add(viewModel.Agent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Re-populate dropdowns
            viewModel.Roles = db.Roles.Select(r => new SelectListItem
            {
                Value = r.RoleID.ToString(),
                Text = r.Role_Name
            }).ToList();

            viewModel.Locations = db.Locations.Select(l => new SelectListItem
            {
                Value = l.Sno.ToString(),
                Text = l.LocationDescription
            }).ToList();

            return View("~/Views/Admin/Agent/Create.cshtml", viewModel);
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var agent = db.Agents.Find(id);
            if (agent == null)
                return HttpNotFound();

            var viewModel = new AgentViewModel
            {
                Agent = agent,
                Roles = db.Roles.Select(r => new SelectListItem
                {
                    Value = r.RoleID.ToString(),
                    Text = r.Role_Name
                }).ToList(),

                Locations = db.Locations.Select(l => new SelectListItem
                {
                    Value = l.Sno.ToString(),
                    Text = l.LocationDescription
                }).ToList()
            };

            return View("~/Views/Admin/Agent/Edit.cshtml", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AgentViewModel viewModel, string[] Agent_WorkArea)
        {
            if (ModelState.IsValid)
            {
                viewModel.Agent.WorkArea = Agent_WorkArea != null ? string.Join(",", Agent_WorkArea) : "";

                viewModel.Agent.LastUpdate = DateTime.Now;
                db.Entry(viewModel.Agent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // repopulate dropdowns on validation error
            viewModel.Roles = db.Roles.Select(r => new SelectListItem
            {
                Value = r.RoleID.ToString(),
                Text = r.Role_Name
            }).ToList();

            viewModel.Locations = db.Locations.Select(l => new SelectListItem
            {
                Value = l.Sno.ToString(),
                Text = l.LocationDescription
            }).ToList();

            return View("~/Views/Admin/Agent/Edit.cshtml", viewModel);
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var agent = db.Agents.Find(id);
            if (agent == null)
                return HttpNotFound();

            db.Agents.Remove(agent);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult History(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var agent = db.Agents.Find(id);
            if (agent == null)
                return HttpNotFound();

            var pno = agent.PNO.StartsWith("P") ? agent.PNO : "P" + agent.PNO;
            var logs = db.HistoryLogs
                         .Where(h => h.PNO == pno)
                         .OrderByDescending(h => h.Date)
                         .ToList();

            ViewBag.AgentName = agent.Name;
            ViewBag.PNO = pno;

            return View("~/Views/Admin/Agent/History.cshtml", logs);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<JsonResult> FetchEmployee(string pno)
        {
            if (string.IsNullOrWhiteSpace(pno))
            {
                return Json(new { success = false, message = "PNO is required." }, JsonRequestBehavior.AllowGet);
            }

            var existingAgent = db.Agents.FirstOrDefault(a => a.PNO == pno);
            if (existingAgent != null)
            {
                return Json(new { exists = true }, JsonRequestBehavior.AllowGet);
            }

            var url = $"https://systemsupport.piac.com.pk/admin/getEmployee.cfm?pno={pno}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(new { success = false, message = "Failed to fetch employee." }, JsonRequestBehavior.AllowGet);
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();
                    var dataArray = JArray.Parse(jsonString);

                    if (!dataArray.Any())
                    {
                        return Json(new { success = false, message = "Employee not found." }, JsonRequestBehavior.AllowGet);
                    }

                    var user = dataArray[0];

                    return Json(new
                    {
                        success = true,
                        user = new
                        {
                            name = user["name"]?.ToString(),
                            email = user["email"]?.ToString(),
                            phone_Num = user["Phone_Num"]?.ToString(),
                            emp_designation = user["Emp_designation"]?.ToString(),
                            image_url = user["image_url"]?.ToString()
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}