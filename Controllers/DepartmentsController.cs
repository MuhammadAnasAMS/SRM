using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PIA_SRM_MVC.Controllers
{
    public class DepartmentsController : Controller
    {
        // GET: Departments
        public ActionResult Departments()
        {
            ViewBag.ScreenTitle = "Departments Management";
            return View();
        }

        public ActionResult Users()
        {
            ViewBag.ScreenTitle = "User Management";
            return View();
        }
        public ActionResult Logs()
        {
            ViewBag.ScreenTitle = "System Logs";
            return View();
        }
    }
}