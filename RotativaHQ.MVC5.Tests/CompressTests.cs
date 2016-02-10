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
    [Trait("RotativaHQ", "preparing zip file with html content")]
    public class CompressTests
    {
        [Fact(DisplayName="should contain zipped html")]
        public void Zipped()
        {
            var html = "<html><head><link href=\"~/Content/Site.css\" rel=\"stylesheet\" /></head>Hello <img src=\"Content/test.png\" /></html>";
            string rootpath = AppDomain.CurrentDomain.BaseDirectory;
            var mockPathResolver = new Mock<IMapPathResolver>();
            mockPathResolver.Setup(x => x.MapPath("Content/test.png"))
                .Returns(Path.Combine(rootpath, "Content", "test.png"));
            mockPathResolver.Setup(x => x.MapPath("~/Content/Site.css"))
                .Returns(Path.Combine(rootpath, "Content", "Site.css"));
            mockPathResolver.Setup(x => x.MapPath("/Content/cheap_diagonal_fabric.png"))
                .Returns(Path.Combine(rootpath, "Content", "test.png"));
            byte[] zippedHtml = Zipper.ZipPage(html, mockPathResolver.Object);


            var fileStream = new MemoryStream(zippedHtml);
            //fileStream.Position = 0;
            using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                Assert.Equal(4, zip.Entries.Count);
                foreach (var entry in zip.Entries)
                {
                    using (var stream = entry.Open())
                    {
                        // do whatever we want with stream
                        // ...
                        //stream.Position = 0;
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
