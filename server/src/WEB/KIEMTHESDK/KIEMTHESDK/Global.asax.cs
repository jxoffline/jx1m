using System.Timers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace KIEMTHESDK
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // Unity.Setup();

            //Timer timer = new Timer(300000);
            //timer.Enabled = true;

            ////  Setup Event Handler for Timer Elapsed Event


            //timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

            //timer.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //  Unity.Setup();

        }
    }
}
