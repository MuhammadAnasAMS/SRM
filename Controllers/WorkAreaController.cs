using System;
using System.Linq;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;

namespace PIA_Admin_Dashboard.Controllers
{
    public class WorkAreaController : AuthenticatedController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // Display all work areas
        public ActionResult Index()
        {
            ViewBag.ScreenTitle = "Work Area Management";

            var locations = db.Locations.ToList();
            ViewBag.Programs = db.Programs.ToList(); // <-- Populate Programs for dropdown

            return View("~/Views/Admin/WorkArea/Index.cshtml", locations);
        }

        // POST: Add new work area
        [HttpPost]
        public ActionResult Add(string LocationID, string LocationDescription, int? ProgramId)
        {
            // Check if the location ID already exists (case-insensitive match)
            var exists = db.Locations.Any(l => l.LocationID.ToLower() == LocationID.ToLower());

            if (exists)
            {
                TempData["ErrorMessage"] = "Work Area ID already exists.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrWhiteSpace(LocationID) && !string.IsNullOrWhiteSpace(LocationDescription))
            {
                var location = new Location
                {
                    LocationID = LocationID,
                    LocationDescription = LocationDescription,
                    ProgramId = ProgramId
                };

                db.Locations.Add(location);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }



        // GET: Edit form
        public ActionResult Edit(int id)
        {
            var location = db.Locations.Find(id); // this uses Sno
            if (location == null)
            {
                return HttpNotFound();
            }

            ViewBag.Programs = db.Programs.ToList();
            return View("~/Views/Admin/WorkArea/Edit.cshtml", location);
        }



        // POST: Edit update
        [HttpPost]
        public ActionResult Edit(Location updatedLocation)
        {
            if (ModelState.IsValid)
            {
                var location = db.Locations.Find(updatedLocation.Sno);
                if (location != null)
                {
                    location.LocationID = updatedLocation.LocationID;
                    location.LocationDescription = updatedLocation.LocationDescription;
                    location.ProgramId = updatedLocation.ProgramId;

                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ViewBag.Programs = db.Programs.ToList(); // In case the model state is invalid
            return View("~/Views/Admin/WorkArea/Edit.cshtml", updatedLocation);
        }

        // Delete location
        public ActionResult Delete(int id)
        {
            var location = db.Locations.Find(id);
            if (location != null)
            {
                db.Locations.Remove(location);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
