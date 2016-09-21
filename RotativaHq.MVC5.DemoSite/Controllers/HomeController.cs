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

            return new  ViewAsPdf();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult HeaderTestHeader()
        {
            return View();
        }

        public ActionResult Simple()
        {
            ViewBag.SomeData = "Ciao";
            return new ViewAsPdf() { 
                HeaderView = "HeaderTestHeader",
                CustomSwitches = "-B 25 -L 20 -R 20 -T 22 -s A4 -O Portrait --print-media-type --header-spacing 3"
            };
            //return View();
        }
        public ActionResult WrongSrc()
        {
            return new ViewAsPdf() { FileName = "Wrongcss.pdf" };
            //return View();
        }

        public ActionResult ScriptJs()
        {
            return new ViewAsPdf();
        }

        public ActionResult InvalidCss()
        {
            return new ViewAsPdf();
            //return View();
        }

        public ActionResult LongImagePath()
        {
            return new ViewAsPdf();
            //return View();
        }

        public ActionResult HeaderTest()
        {
            return new ViewAsPdf() 
            {
                HeaderView = "HeaderTestHeader",
                FooterView = "HeaderTestHeader",
                CustomSwitches = ""
            };
        }
    }
}