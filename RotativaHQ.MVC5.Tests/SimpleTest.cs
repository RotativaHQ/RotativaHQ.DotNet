using Moq;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Xunit;

namespace RotativaHQ.MVC5.Tests
{
    public class BaseNugetPackageTests : IDisposable
    {
        private IWebDriver selenium;
        private StringBuilder verificationErrors;
        protected PdfTester PdfTester;
        protected IWebDriver Driver;
        protected WebClient WebClient;

        public BaseNugetPackageTests()
        {
            Driver = new OpenQA.Selenium.IE.InternetExplorerDriver();
            Driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 10));
            verificationErrors = new StringBuilder();
            var rotativaDemoUrl = ConfigurationManager.AppSettings["RotativaDemoUrl"];
            Driver.Navigate().GoToUrl(rotativaDemoUrl);
            WebClient = new WebClient();
            PdfTester = new PdfTester();
        }

        public void Dispose()
        {
            if (WebClient != null) WebClient.Dispose();
            if (Driver != null) Driver.Quit();
        }
    }

    [Trait("RotativaHQ", "calling a simple page")]
    public class SimpleTest
    {
        [Fact(DisplayName="should return the link to valid pdf")]
        public void ValidPdf()
        {
            var rotativaDemoUrl = ConfigurationManager.AppSettings["RotativaDemoUrl"];
            var pdfLink = rotativaDemoUrl + "/Home/Simple";
            using (var webClient = new WebClient())
            {
                
                var pdf = webClient.DownloadData(new Uri(pdfLink));
                var pdfTester = new PdfTester();
                pdfTester.LoadPdf(pdf);
                Assert.True(pdfTester.PdfIsValid);
                Assert.True(pdfTester.PdfContains("page"));
            }
        }
    }
}
