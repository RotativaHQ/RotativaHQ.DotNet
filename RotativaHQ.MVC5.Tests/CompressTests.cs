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
            mockPathResolver.Setup(x => x.MapPath("", "Content/test.png"))
                .Returns(Path.Combine(rootpath, "Content", "test.png"));
            mockPathResolver.Setup(x => x.MapPath("", "~/Content/Site.css"))
                .Returns(Path.Combine(rootpath, "Content", "Site.css"));
            mockPathResolver.Setup(x => x.MapPath("~/Content/Site.css", "/Content/cheap_diagonal_fabric.png"))
                .Returns(Path.Combine(rootpath, "Content", "cheap_diagonal_fabric.png"));
            byte[] zippedHtml = Zipper.ZipPage(html, mockPathResolver.Object, "http://localhost:57399", "");


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

        [Fact(DisplayName="should contain zipped css bundle")]
        public void Bundle()
        {
            var html = "<html><head><link href=\"/Content/css?v=peG2vCX8wlIEw2lPUnRL6uPAxina05CUT_UoTb_UXfw1\" rel=\"stylesheet\"/><script src=\"/bundles/modernizr?v=wBEWDufH_8Md-Pbioxomt90vm6tJN2Pyy9u9zHtWsPo1\"></script></head>Hello <img src=\"Content/test.png\" /></html>";
            string rootpath = AppDomain.CurrentDomain.BaseDirectory;
            var mockPathResolver = new Mock<IMapPathResolver>();
            mockPathResolver.Setup(x => x.MapPath("", "Content/test.png"))
                .Returns(Path.Combine(rootpath, "Content", "test.png"));
            mockPathResolver.Setup(x => x.MapPath("", "../fonts/glyphicons-halflings-regular.eot"))
                .Returns(Path.Combine(rootpath, "fonts", "glyphicons-halflings-regular.eot"));
            mockPathResolver.Setup(x => x.MapPath("", "../fonts/glyphicons-halflings-regular.svg"))
                .Returns(Path.Combine(rootpath, "fonts", "glyphicons-halflings-regular.svg"));
            mockPathResolver.Setup(x => x.MapPath("", "../fonts/glyphicons-halflings-regular.ttf"))
                .Returns(Path.Combine(rootpath, "fonts", "glyphicons-halflings-regular.ttf"));
            mockPathResolver.Setup(x => x.MapPath("", "../fonts/glyphicons-halflings-regular.woff"))
                .Returns(Path.Combine(rootpath, "fonts", "glyphicons-halflings-regular.woff"));
            mockPathResolver.Setup(x => x.MapPath("", "../../images/cheap_diagonal_fabric.png"))
                .Returns(Path.Combine(rootpath, "Content", "test.png"));
            byte[] zippedHtml = Zipper.ZipPage(html, mockPathResolver.Object, "http://localhost:57399", "");


            var fileStream = new MemoryStream(zippedHtml);
            //fileStream.Position = 0;
            using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                Assert.Equal(4, zip.Entries.Count);
                Assert.True(zip.Entries.ToList().Exists(x => x.Name.EndsWith(".css")), "no css file");
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
