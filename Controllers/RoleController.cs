using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PIA_Admin_Dashboard.Models;
using System.Data.Entity;

namespace PIA_Admin_Dashboard.Controllers
{
    public class RoleController : AuthenticatedController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.ScreenTitle = "Role Management";

            // Load programs for the dropdown filter
            ViewBag.Programs = db.Programs.OrderBy(p => p.Name).ToList();

            // Get all roles and manually set user counts
            var roles = db.Roles.ToList();
            foreach (var role in roles)
            {
                // Count users assigned to this role (assuming you have an Agents table with RoleId)
                // Adjust this query based on your actual user/agent table structure
                role.UserCount = db.Agents.Count(a => a.RoleId == role.RoleID);
                role.IsActive = true; // Set default status
            }

            return View("~/Views/Admin/Role/Index.cshtml", roles);
        }

        public ActionResult Create()
        {
            ViewBag.ScreenTitle = "Create Role";

            // Load programs for the dropdown
            ViewBag.Programs = db.Programs.OrderBy(p => p.Name).ToList();

            var viewModel = new RoleViewModel
            {
                Role = new Role
                {
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    program_id = 1 // Set default program_id, adjust as needed
                }
            };
            return View("~/Views/Admin/Role/Create.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RoleViewModel viewModel, string[] privileges)
        {
            if (ModelState.IsValid)
            {
                // Join selected privileges into a single string
                if (privileges != null && privileges.Any())
                {
                    viewModel.Role.Privilege = string.Join(",", privileges);
                }

                // Set default values
                viewModel.Role.program_id = viewModel.Role.program_id ?? 1; // Default program_id

                db.Roles.Add(viewModel.Role);
                db.SaveChanges();

                TempData["Success"] = "Role created successfully!";
                return RedirectToAction("Index");
            }

            // Reload programs for dropdown in case of validation errors
            ViewBag.Programs = db.Programs.OrderBy(p => p.Name).ToList();

            TempData["Error"] = "Failed to create role. Please check your input.";
            return View("~/Views/Admin/Role/Create.cshtml", viewModel);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var role = db.Roles.Find(id);
            if (role == null)
                return HttpNotFound();

            ViewBag.ScreenTitle = "Edit Role";

            // Load programs for the dropdown
            ViewBag.Programs = db.Programs.OrderBy(p => p.Name).ToList();

            var viewModel = new RoleViewModel
            {
                Role = role,
                SelectedPrivileges = !string.IsNullOrEmpty(role.Privilege)
                    ? role.Privilege.Split(',')
                    : new string[0]
            };

            return View("~/Views/Admin/Role/Edit.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RoleViewModel viewModel, string[] privileges)
        {
            if (ModelState.IsValid)
            {
                // Join selected privileges into a single string
                if (privileges != null && privileges.Any())
                {
                    viewModel.Role.Privilege = string.Join(",", privileges);
                }
                else
                {
                    viewModel.Role.Privilege = "";
                }

                db.Entry(viewModel.Role).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Role updated successfully!";
                return RedirectToAction("Index");
            }

            // Reload programs for dropdown in case of validation errors
            ViewBag.Programs = db.Programs.OrderBy(p => p.Name).ToList();

            TempData["Error"] = "Failed to update role. Please check your input.";
            viewModel.SelectedPrivileges = !string.IsNullOrEmpty(viewModel.Role.Privilege)
                ? viewModel.Role.Privilege.Split(',')
                : new string[0];

            return View("~/Views/Admin/Role/Edit.cshtml", viewModel);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var role = db.Roles.Find(id);
            if (role == null)
                return HttpNotFound();

            // Check if role is being used by any agents
            var userCount = db.Agents.Count(a => a.RoleId == id);
            if (userCount > 0)
            {
                TempData["Error"] = $"Cannot delete role. It is currently assigned to {userCount} user(s).";
                return RedirectToAction("Index");
            }

            db.Roles.Remove(role);
            db.SaveChanges();

            TempData["Success"] = "Role deleted successfully!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}