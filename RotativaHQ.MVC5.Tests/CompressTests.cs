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
        [Fact(DisplayName="should contain zipped heml")]
        public void Zipped()
        {
            var html = "<html>Hello</html>";

            byte[] zippedHtml = Zipper.ZipPage(html);


            var fileStream = new MemoryStream(zippedHtml);
            //fileStream.Position = 0;
            using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    using (var stream = entry.Open())
                    {
                        // do whatever we want with stream
                        // ...
                        //stream.Position = 0;
                        var sr = new StreamReader(stream);
                        var myStr = sr.ReadToEnd();
                        Assert.Equal(html, myStr);
                    }
                }
            }
        }
    }
}
