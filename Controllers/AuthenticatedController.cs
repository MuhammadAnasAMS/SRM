using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PIA_Admin_Dashboard.Controllers
{
    public class AuthenticatedController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["AgentUid"] == null)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            //  Prevent browser from caching protected pages
            HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            HttpContext.Response.Cache.SetNoStore();

            base.OnActionExecuting(filterContext);
        }


    }
}