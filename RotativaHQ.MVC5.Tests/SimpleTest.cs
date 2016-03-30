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

    public abstract class BaseWebClientTest
    {
        protected readonly static string RotativaDemoUrl = ConfigurationManager.AppSettings["RotativaDemoUrl"];
        private PdfTester pdfTester { get; set; }

        public BaseWebClientTest()
        {
            pdfTester = new PdfTester();
        }

        protected async Task GetPdfForAction(string action, Action<PdfTester> assertCallback)
        {
            var pdfLink = RotativaDemoUrl + action;
            using (var webClient = new WebClient())
            {
                var pdf = await webClient.DownloadDataTaskAsync(new Uri(pdfLink));
                pdfTester.LoadPdf(pdf);
                Assert.True(pdfTester.PdfIsValid, "it's not a valid pdf");
                assertCallback(pdfTester);
            }
        }

        protected async Task VerifyPdfForAction(string action, string verifyText)
        {
            await GetPdfForAction(action, pdfTester => {
                Assert.True(pdfTester.PdfContains(verifyText), "it doesn't contain searched text");
            });
        }
    }

    [Trait("RotativaHQ", "calling a simple page")]
    public class SimpleTest: BaseWebClientTest
    {
        [Fact(DisplayName="should return the link to valid pdf")]
        public async Task ValidPdf()
        {
            await VerifyPdfForAction("/Home/Simple", "page"); 
        }
    }

    [Trait("RotativaHQ", "calling a simple page with header")]
    public class SimplePagedTest : BaseWebClientTest
    {
        [Fact(DisplayName = "should return the link to paged pdf")]
        public async Task ValidPdf()
        {
            await VerifyPdfForAction("/Home/HeaderTest", "page 1 of 1");
        }
    }

    [Trait("RotativaHQ", "calling a page with invalid css url")]
    public class InvalidCssTest: BaseWebClientTest
    {
        [Fact(DisplayName="should return the link to valid pdf")]
        public async Task ValidPdf()
        {
            await VerifyPdfForAction("/Home/InvalidCss", "page"); 
        }
    }

    [Trait("RotativaHQ", "calling a page with Javascript script and small (100) delay")]
    public class ScriptJsTest
    {
        [Fact(DisplayName="should return the link to valid pdf")]
        public void ValidPdf()
        {
            var rotativaDemoUrl = ConfigurationManager.AppSettings["RotativaDemoUrl"];
            var pdfLink = rotativaDemoUrl + "/Home/ScriptJs";
            using (var webClient = new WebClient())
            {
                var pdf = webClient.DownloadData(new Uri(pdfLink));
                var pdfTester = new PdfTester();
                pdfTester.LoadPdf(pdf);
                Assert.True(pdfTester.PdfIsValid, "it's not a valid pdf");
                Assert.True(pdfTester.PdfContains("whooah"), "it doesn't contain searched text");
            }
        }
    }
}
