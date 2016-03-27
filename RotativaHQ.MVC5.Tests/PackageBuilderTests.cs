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
    [Trait("PackageBuilder", "get assets for html with one png image")]
    public class PackageBuilderTests : BasePackageTest
    {
        [Fact(DisplayName="should return one png asset")]
        public void ImageAsset()
        {
            var html = "<html><head></head><body>Hello <img src=\"Content/test.png\" /></body></html>";

            List<Asset> assets = PackageBuilder.GetHtmlAssets(html);

            Assert.Equal(1, assets.Count);
            Assert.Equal("png", assets.First().Suffix);
        }
    }
    [Trait("PackageBuilder", "get assets for html with one local png image and one external image")]
    public class PackageBuilderLocalTests: BasePackageTest
    {
        [Fact(DisplayName = "should return one png asset")]
        public void ImageAsset()
        {
            var html = @"<html><head></head><body>Hello <img src=""Content/test.png"" />
                            <img src=""//Content/test.png"" /></body></html>";

            List<Asset> assets = PackageBuilder.GetHtmlAssets(html);

            Assert.Equal(1, assets.Count);
            Assert.Equal("png", assets.First().Suffix);
        }
    }

    [Trait("PackageBuilder", "get assets for html with one css link rel and one image")]
    public class PackageBuilder2Tests: BasePackageTest
    {
        [Fact(DisplayName="should return one css asset and one image")]
        public void ImageAsset()
        {
            var html = "<html><head><link href=\"/Content/Css?sadsdadas\" rel=\"stylesheet\" /></head><body>Hello <img src=\"Content/test.png\" /></body></html>";

            List<Asset> assets = PackageBuilder.GetHtmlAssets(html);

            Assert.Equal(2, assets.Count);
            Assert.NotNull(assets.FirstOrDefault(c => c.Suffix == "png"));
            Assert.NotNull(assets.FirstOrDefault(c => c.Suffix == "css"));
        }
    }

    [Trait("PackageBuilder", "get assets for html with one js script and one image")]
    public class PackageBuilder3Tests: BasePackageTest
    {
        [Fact(DisplayName = "should return one css asset and one image")]
        public void ImageAsset()
        {
            var html = "<html><head><script src=\"/Content/Js?sadsdadas\" type=\"javascript\"></script></head><body>Hello <img src=\"Content/test.png\" /></body></html>";

            List<Asset> assets = PackageBuilder.GetHtmlAssets(html);

            Assert.Equal(2, assets.Count);
            Assert.NotNull(assets.FirstOrDefault(c => c.Suffix == "png"));
            Assert.NotNull(assets.FirstOrDefault(c => c.Suffix == "js"));
        }
    }

    [Trait("PackageBuilder", "get assets for css with one png image")]
    public class PackageBuilderCss1Tests: BasePackageTest
    {
        [Fact(DisplayName="should return one png asset")]
        public void ImageAsset()
        {
            var css = @"
                body {
                    padding-top: 50px;
				    padding-bottom: 20px;
				    background-image: url(/Content/cheap_diagonal_fabric.png?stupid=iloveyou);
				}
            ";

            List<Asset> assets = PackageBuilder.GetCssAssets(css);

            Assert.Equal(1, assets.Count);
            Assert.Equal("png", assets.First().Suffix);
        }
    }

    [Trait("PackageBuilder", "filling all assets")]
    public class FillingAssets: BasePackageTest
    {
        [Fact(DisplayName="should return assets with content")]
        public void FillAll()
        {
            var html = @"<html><head><link href=""/Content/Site.css"" rel=""stylesheet"" /></head><body>Hello <img src=""../Content/test.png"" />
                            <img src=""//Content/test.png"" /></body></html>";

            List<AssetContent> assets = PackageBuilder.GetAssetsContents(html, PagePath, "index");

            Assert.Equal(4, assets.Count);
        }
    }

    [Trait("PackageBuilder", "getting the zip archive")]
    public class ZippingAssets : BasePackageTest
    {
        [Fact(DisplayName = "should return assets with content")]
        public void FillAll()
        {
            var html = @"<html><head><link href=""/Content/Site.css"" rel=""stylesheet"" /></head><body>Hello <img src=""/Content/test.png"" />
                            <img src=""//Content/test.png"" /></body></html>";

            List<AssetContent> assets = PackageBuilder.GetAssetsContents(html, PagePath, "index");
            var archive = PackageBuilder.GetPackage(assets);
            //var oldArchive = Zipper.ZipPage(html, MapPathResolver, RootPath, PagePath);

            Assert.Equal(4, assets.Count);
            var fileStream = new MemoryStream(archive);
            //var fileStream2 = new MemoryStream(oldArchive);
            //fileStream.Position = 0;
            using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
            //using (var zip2 = new ZipArchive(fileStream2, ZipArchiveMode.Read))
            {
                Assert.Equal(4, zip.Entries.Count);
                //Assert.Equal(zip.Entries.Count, zip2.Entries.Count);
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
                            Assert.Equal(2, doc.Images.Count());
                            Assert.NotEqual("/Content/test.png", doc.Images[0].Attributes["src"].Value);
                            //Assert.Equal(html, myStr);
                        }
                        if (entry.Name.ToLower().EndsWith("css"))
                        {
                            var myStrCss = sr.ReadToEnd();
                            Assert.Contains("background-image", myStrCss);
                            //Assert.Equal(html, myStr);
                        }
                    }
                }
            }
        }
    }

    public abstract class BasePackageTest
    {
        protected PackageBuilder PackageBuilder { get; set; }
        protected string PagePath { get; set; }
        protected IMapPathResolver MapPathResolver { get; set; }

        protected string RootPath { get; set; }

        public BasePackageTest()
        {
            RootPath = AppDomain.CurrentDomain.BaseDirectory;
            PagePath = "Home/Simple";
            var mockPathResolver = new Mock<IMapPathResolver>();
            mockPathResolver.Setup(x => x.MapPath(PagePath, "../content/test.png"))
                .Returns(Path.Combine(RootPath, "Content", "test.png"));
            mockPathResolver.Setup(x => x.MapPath(PagePath, "/content/test.png"))
                .Returns(Path.Combine(RootPath, "Content", "test.png"));
            mockPathResolver.Setup(x => x.MapPath(PagePath, "/content/site.css"))
                .Returns(Path.Combine(RootPath, "Content", "Site.css"));
            mockPathResolver.Setup(x => x.MapPath("/content/site.css", "/content/cheap_diagonal_fabric.png"))
                .Returns(Path.Combine(RootPath, "Content", "cheap_diagonal_fabric.png"));
            MapPathResolver = mockPathResolver.Object;
            PackageBuilder = new PackageBuilder(mockPathResolver.Object, RootPath);

        }
    }
}
