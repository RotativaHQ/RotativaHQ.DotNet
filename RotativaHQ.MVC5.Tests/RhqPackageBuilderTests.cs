using Moq;
using RotativaHQ.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RotativaHQ.MVC5.Tests
{
    [Trait("RhqPackageBuilder", "packaging html with image and css background")]
    public class RhqPackageBuilderTests
    {
        //[Fact(DisplayName="should output package with 4 elements")]
        public void PackageIt()
        {
            var html = "<html><head><link href=\"~/Content/Site.css\" rel=\"stylesheet\" /></head>Hello <img src=\"Content/test.png\" /></html>";
            var pagePath = "/Home/Simple";
            string rootpath = AppDomain.CurrentDomain.BaseDirectory;
            var mockPathResolver = new Mock<IMapPathResolver>();
            mockPathResolver.Setup(x => x.MapPath(pagePath, "Content/test.png"))
                .Returns(Path.Combine(rootpath, "Content", "test.png"));
            mockPathResolver.Setup(x => x.MapPath(pagePath, "~/Content/Site.css"))
                .Returns(Path.Combine(rootpath, "Content", "Site.css"));
            mockPathResolver.Setup(x => x.MapPath("~/Content/Site.css", "/Content/cheap_diagonal_fabric.png"))
                .Returns(Path.Combine(rootpath, "Content", "cheap_diagonal_fabric.png"));
            byte[] zippedHtml;
            using (var packageBuilder = new RhqPackageBuilder(mockPathResolver.Object, ""))
            {
                packageBuilder.AddPage(html, "http://localhost:57399", pagePath);

                zippedHtml = packageBuilder.GetZippedPage();
            }
            var fileStream = new MemoryStream(zippedHtml);
            //fileStream.Position = 0;
            using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                Assert.Equal(4, zip.Entries.Count);
                foreach (var entry in zip.Entries)
                {
                    using (var stream = entry.Open())
                    {
                        var sr = new StreamReader(stream);
                        if (entry.Name.ToLower() == "index.html")
                        {
                            var myStr = sr.ReadToEnd();
                            var parser = new AngleSharp.Parser.Html.HtmlParser();
                            var doc = parser.Parse(myStr);
                            Assert.Equal(1, doc.Images.Count());
                            Assert.NotEqual("Content/test.png", doc.Images[0].Attributes["src"].Value);
                            //Assert.Equal(html, myStr);
                        }
                    }
                }
            }
        }
    }
}
