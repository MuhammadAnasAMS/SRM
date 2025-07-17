using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PIA_Admin_Dashboard.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Dashboard()
        {
            ViewBag.ScreenTitle = "Dashboard";
            return View();
        }
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

        public ActionResult Complaints()
        {
            ViewBag.ScreenTitle = "Complaints Handling";
            return View();
        }

        public ActionResult Logs()
        {
            ViewBag.ScreenTitle = "System Logs";
            return View();
        }
    }
}
