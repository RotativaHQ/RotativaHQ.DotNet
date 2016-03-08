using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RotativaHQ.MVC5;

namespace RotativaHq.MVC5.DemoSite.Controllers
{
    public class TestController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            return Request.CreatePdfResponse("~/Views/Home/Simple.cshtml", filename: "simple.pdf");
        }

        public dynamic Get(int id)
        {
            var pdfUrl = PdfHelper.GetPdfUrl("~/Views/Home/Simple.cshtml");
            return new { PdfUrl = pdfUrl };
        }
    }
}
