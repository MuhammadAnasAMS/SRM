using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;
using System.Collections.Generic;
using System;

namespace PIA_Admin_Dashboard.Controllers
{
    public class UserController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.Department).ToList();
            return View("~/Views/Admin/User/Index.cshtml", users);
        }

        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "Name");
            return View("~/Views/Admin/User/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "Name", user.DepartmentId);
            return View("~/Views/Admin/User/Create.cshtml", user);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "Name", user.DepartmentId);
            return View("~/Views/Admin/User/Edit.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "Name", user.DepartmentId);
            return View("~/Views/Admin/User/Edit.cshtml", user);
        }

        public ActionResult Delete(int? id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Include(u => u.Department).FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return HttpNotFound();

            return View("~/Views/Admin/User/Details.cshtml", user);
        }

        [HttpGet]
        public JsonResult FetchEmployee(string pno)
        {
            if (string.IsNullOrEmpty(pno))
                return Json(new { success = false, message = "PNO missing." }, JsonRequestBehavior.AllowGet);

            var existing = db.Users.FirstOrDefault(u => u.EmployeeId == pno);
            if (existing != null)
                return Json(new { exists = true, message = "Employee already exists." }, JsonRequestBehavior.AllowGet);

            using (var client = new System.Net.WebClient())
            {
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                    string url = "https://systemsupport.piac.com.pk/admin/getEmployee.cfm?pno=" + pno;
                    var json = client.DownloadString(url);

                    if (string.IsNullOrWhiteSpace(json))
                        return Json(new { success = false, message = "API returned empty response." }, JsonRequestBehavior.AllowGet);

                    // ❗ Check for {"Error":"Invalid PNO"} before deserialization
                    if (json.TrimStart().StartsWith("{") && json.Contains("\"Error\""))
                        return Json(new { success = false, message = "Invalid PNO." }, JsonRequestBehavior.AllowGet);

                    List<ApiEmployeeResponse> apiResponse = null;

                    try
                    {
                        apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ApiEmployeeResponse>>(json);
                    }
                    catch
                    {
                        var singleResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiEmployeeResponse>(json);
                        if (singleResponse != null && !string.IsNullOrWhiteSpace(singleResponse.pno))
                            apiResponse = new List<ApiEmployeeResponse> { singleResponse };
                    }

                    if (apiResponse == null || !apiResponse.Any() || string.IsNullOrWhiteSpace(apiResponse.First().pno))
                    {
                        return Json(new { success = false, message = "Employee not found or invalid response." }, JsonRequestBehavior.AllowGet);
                    }

                    var emp = apiResponse.First();

                    return Json(new
                    {
                        success = true,
                        user = new
                        {
                            Name = emp.name,
                            EmployeeId = emp.pno,
                            Email = emp.email,
                            Mobile = emp.Phone_Num,
                            Designation = emp.Emp_designation
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "API error: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
