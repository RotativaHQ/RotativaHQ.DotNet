using System.Web;
using System.Web.Mvc;

namespace RotativaHq.MVC5.DemoSite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
