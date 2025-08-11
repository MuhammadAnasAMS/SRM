using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PIA_Admin_Dashboard.Controllers
{
 
        public class ErrorController : Controller
        {
            public ActionResult PageNotFound()
            {
                Response.StatusCode = 404;
                return View("Error404");
            }

            public ActionResult General()
            {
                return View("GeneralError"); // optional
            }
        }

 }
