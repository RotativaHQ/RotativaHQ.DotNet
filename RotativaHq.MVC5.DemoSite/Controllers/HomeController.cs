using RotativaHQ.MVC5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RotativaHq.MVC5.DemoSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return new ViewAsPdf();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Simple()
        {
            return new ViewAsPdf();
            //return View();
        }
        public ActionResult ScriptJs()
        {
            return new ViewAsPdf();
            //return View();
        }
    }
}