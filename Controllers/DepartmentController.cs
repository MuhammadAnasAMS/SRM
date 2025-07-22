using System;
using System.Linq;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;

public class DepartmentController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    public ActionResult Index()
    {
        ViewBag.ScreenTitle = "Departments Management";
        var departments = db.Departments.ToList();
        return View("~/Views/Admin/Department/Index.cshtml", departments);
    }

    [HttpPost]
    public ActionResult Add(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var dept = new Department
            {
                Name = name,
                CreatedAt = DateTime.Now
            };
            db.Departments.Add(dept);
            db.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    public ActionResult Edit(int id)
    {
        var department = db.Departments.Find(id);
        if (department == null)
            return HttpNotFound();

        return View("~/Views/Admin/Department/Edit.cshtml", department);
    }

    [HttpPost]
    public ActionResult Edit(Department updatedDept)
    {
        if (ModelState.IsValid)
        {
            var dept = db.Departments.Find(updatedDept.DepartmentId);
            if (dept != null)
            {
                dept.Name = updatedDept.Name;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        return View("~/Views/Admin/Department/Edit.cshtml", updatedDept);
    }

    public ActionResult Delete(int id)
    {
        var dept = db.Departments.Find(id);
        if (dept != null)
        {
            db.Departments.Remove(dept);
            db.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}