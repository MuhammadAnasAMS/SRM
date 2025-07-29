using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;
using System.Data.Entity;

namespace PIA_Admin_Dashboard.Controllers
{
    public class AgentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var agents = db.Agents.ToList();
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
            return View("~/Views/Admin/Agent/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Agent agent)
        {
            if (ModelState.IsValid)
            {
                agent.LastUpdate = DateTime.Now;
                db.Agents.Add(agent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("~/Views/Admin/Agent/Create.cshtml", agent);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var agent = db.Agents.Find(id);
            if (agent == null)
                return HttpNotFound();

            return View("~/Views/Admin/Agent/Edit.cshtml", agent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Agent agent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(agent).State = EntityState.Modified;
                agent.LastUpdate = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("~/Views/Admin/Agent/Edit.cshtml", agent);
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


    }
}