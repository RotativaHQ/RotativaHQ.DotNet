using AngleSharp.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RotativaHQ.Core
{
    public class Zipper
    {
        public static byte[] ZipPage(string html, IMapPathResolver mapPathResolver)
        {
            var parser = new AngleSharp.Parser.Html.HtmlParser();
            var doc = parser.Parse(html);
            var images = doc.Images;
            var serialImages = new Dictionary<string, string>();
            foreach (var image in images)
            {
                var canSerialize = false;
                var src = image.Attributes["src"].Value;
                if (src.ToLower().StartsWith("http"))
                {
                    Uri url = new Uri(src);
                    var hostname = url.Host;
                    if (IsLocalhost(hostname))
                    {
                        src = url.LocalPath;
                        canSerialize = true;
                    }
                }
                else
                    canSerialize = true;
                if (canSerialize)
                {
                    var suffix = src.Split('.').Last();
                    var newSrc = Guid.NewGuid().ToString().Replace("-", "") + "." + suffix;
                    image.Attributes["src"].Value = newSrc;
                    serialImages.Add(src, newSrc);
                }
            }
            var newHtml = doc.ToHtml(new HtmlMarkupFormatter());
            using (var ms = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
    			{
    			    //foreach (var attachment in attachmentFiles)
    			    {
    			        var entry = zipArchive.CreateEntry("index.html", CompressionLevel.Fastest);
    			        
                        using (StreamWriter writer = new StreamWriter(entry.Open()))
                        {
                            writer.Write(newHtml);
                        }
    			    }
                    foreach (var serialImage in serialImages)
                    {
                        var nentry = zipArchive.CreateEntry(serialImage.Value, CompressionLevel.Fastest);
                        using (var writer = new BinaryWriter(nentry.Open()))
                        {
                            //var image = File.ReadAllBytes(Path.Combine(rootPath,"Content", "test.png"));
                            var path = mapPathResolver.MapPath(serialImage.Key);
                            var image = File.ReadAllBytes(path);
                            writer.Write(image);
                        }
                    }
    			}
                return ms.ToArray();
            }
        }
        private static bool IsLocalhost(string hostNameOrAddress)
        {
            if (string.IsNullOrEmpty(hostNameOrAddress))
                return false;

            try
            {
                // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(hostNameOrAddress);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                // test if any host IP is a loopback IP or is equal to any local IP
                return hostIPs.Any(hostIP => IPAddress.IsLoopback(hostIP) || localIPs.Contains(hostIP));
            }
            catch
            {
                return false;
            }
        }
    }
}
