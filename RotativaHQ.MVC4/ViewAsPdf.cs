using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RotativaHQ.MVC4
{
    public class ViewAsPdf : AsPdfResultBase
    {
        private string _viewName;

        public string ViewName
        {
            get { return _viewName ?? string.Empty; }
            set { _viewName = value; }
        }

        private string _masterName;

        public string MasterName
        {
            get { return _masterName ?? string.Empty; }
            set { _masterName = value; }
        }

        public string HeaderView { get; set; }
        public string FooterView { get; set; }

        public object Model { get; set; }

        public ViewAsPdf()
        {
            MasterName = string.Empty;
            ViewName = string.Empty;
            Model = null;
        }

        public ViewAsPdf(string viewName)
            : this()
        {
            ViewName = viewName;
        }

        public ViewAsPdf(object model)
            : this()
        {
            Model = model;
        }

        public ViewAsPdf(string viewName, object model)
            : this()
        {
            ViewName = viewName;
            Model = model;
        }

        public ViewAsPdf(string viewName, string masterName, object model)
            : this(viewName, model)
        {
            MasterName = masterName;
        }

        protected virtual ViewEngineResult GetView(ControllerContext context, string viewName, string masterName)
        {
            return ViewEngines.Engines.FindView(context, viewName, masterName);
        }

        protected override string CallTheDriver(ControllerContext context)
        {
            context.Controller.ViewData.Model = Model;

            // use action name if the view name was not provided
            if (string.IsNullOrEmpty(ViewName))
                ViewName = context.RouteData.GetRequiredString("action");
            StringBuilder html = new StringBuilder();
            StringBuilder header = new StringBuilder();
            StringBuilder footer = new StringBuilder();

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = GetView(context, ViewName, MasterName);

                // view not found, throw an exception with searched locations
                if (viewResult.View == null)
                {
                    var locations = new StringBuilder();
                    locations.AppendLine();

                    foreach (string location in viewResult.SearchedLocations)
                    {
                        locations.AppendLine(location);
                    }

                    throw new InvalidOperationException(string.Format("The view '{0}' or its master was not found, searched locations: {1}", ViewName, locations));
                }

                var viewContext = new ViewContext(context, viewResult.View, context.Controller.ViewData, context.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                html = sw.GetStringBuilder();
            }

            if (!string.IsNullOrEmpty(HeaderView))
            {
                using (var hw = new StringWriter())
                {
                    ViewEngineResult headerViewResult = GetView(context, HeaderView, MasterName);
                    if (headerViewResult != null)
                    {
                        var viewContext = new ViewContext(context, headerViewResult.View, context.Controller.ViewData, context.Controller.TempData, hw);
                        headerViewResult.View.Render(viewContext, hw);

                        header = hw.GetStringBuilder();
                        ExtraSwitches += " --header-html header.html";
                    }
                }
            }

            if (!string.IsNullOrEmpty(FooterView))
            {
                using (var hw = new StringWriter())
                {
                    ViewEngineResult footerViewResult = GetView(context, FooterView, MasterName);
                    if (footerViewResult != null)
                    {
                        var viewContext = new ViewContext(context, footerViewResult.View, context.Controller.ViewData, context.Controller.TempData, hw);
                        footerViewResult.View.Render(viewContext, hw);

                        footer = hw.GetStringBuilder();
                        ExtraSwitches += " --footer-html footer.html";
                    }
                }
            }
            // replace href and src attributes with full URLs
            var apiKey = ConfigurationManager.AppSettings["RotativaKey"].ToString();
            var client = new RotativaHqClient(apiKey);
            var fileUrl = client.GetPdfUrl(GetConvertOptions(), html.ToString(), this.FileName, header.ToString(), footer.ToString());
            return fileUrl;
        }
    }
}
