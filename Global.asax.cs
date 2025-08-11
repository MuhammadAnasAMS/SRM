using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PIA_Admin_Dashboard
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }


        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            var httpException = ex as HttpException;

            if (httpException != null && httpException.GetHttpCode() == 404)
            {
                Response.Clear();
                Server.ClearError();
                Response.Redirect("~/Error/PageNotFound");
            }
        }


    }
}
