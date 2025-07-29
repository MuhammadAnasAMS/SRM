using System;
using System.Linq;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;

namespace PIA_Admin_Dashboard.Controllers
{
    public class ProgramController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.ScreenTitle = "Programs Management";
            var programs = db.Programs.ToList();
            return View("~/Views/Admin/Program/Index.cshtml", programs);
        }

        [HttpPost]
        public ActionResult Add(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var program = new Program
                {
                    Name = name,
                    ProgramUid = Guid.NewGuid().ToString()
                };
                db.Programs.Add(program);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var program = db.Programs.Find(id);
            if (program == null)
                return HttpNotFound();

            return View("~/Views/Admin/Program/Edit.cshtml", program);
        }

        [HttpPost]
        public ActionResult Edit(Program updatedProgram)
        {
            if (ModelState.IsValid)
            {
                var program = db.Programs.Find(updatedProgram.ProgramId);
                if (program != null)
                {
                    program.Name = updatedProgram.Name;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View("~/Views/Admin/Program/Edit.cshtml", updatedProgram);
        }

        public ActionResult Delete(int id)
        {
            var program = db.Programs.Find(id);
            if (program != null)
            {
                db.Programs.Remove(program);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
