using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PIA_SRM_MVC.Controllers
{
    public class ComplaintsController : Controller
    {
        // GET: Complaints
        public ActionResult Complaints()
        {
            ViewBag.ScreenTitle = "Complaints Handling";
            return View();
        }
    }
}