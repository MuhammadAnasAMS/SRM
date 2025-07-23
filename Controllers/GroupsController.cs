using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PIA_Admin_Dashboard.Controllers
{
    public class GroupsController : Controller
    {
        // GET: Groups
        public ActionResult Index()
        {
            ViewBag.ScreenTitle = "Groups Management";
            return View("~/Views/Admin/Groups/Index.cshtml");
        }
    }
}