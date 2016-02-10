using AngleSharp.Dom;
using AngleSharp.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RotativaHQ.Core
{
    public static class Zipper
    {
        private static string ReturnLocalPath(string url)
        {
            if (!url.ToLower().StartsWith("http"))
            {
                return url;
            }
            Uri uri = new Uri(url);
            var hostname = uri.Host;
            if (IsLocalhost(hostname))
            {
                return uri.LocalPath;
            }
            return string.Empty;
        }

        // TODO: refactor please. Method says Add but in fact it also modifies the elements, 
        // not clear side effect
        private static void AddSerializedAssets(
            this Dictionary<string, string> serialAssets, 
            IEnumerable<IElement> elements, 
            string uriAttribute
            )
        {
            foreach (var image in elements)
            {
                //var canSerialize = false;
                var src = image.Attributes[uriAttribute].Value;
                //if (src.ToLower().StartsWith("http"))
                //{
                //    Uri url = new Uri(src);
                //    var hostname = url.Host;
                //    if (IsLocalhost(hostname))
                //    {
                //        src = url.LocalPath;
                //        canSerialize = true;
                //    }
                //}
                //else
                //    canSerialize = true;
                var localPath = ReturnLocalPath(src);
                if (localPath != string.Empty)
                {
                    var suffix = src.Split('.').Last();
                    var newSrc = Guid.NewGuid().ToString().Replace("-", "") + "." + suffix;
                    image.Attributes[uriAttribute].Value = newSrc;
                    serialAssets.Add(src, newSrc);
                }
            }
        }

        public static List<string> ExtaxtUrlsFromStyle(string styleContent)
        {
            var list = new List<string>();
            var reg = @"url *(?:\(['|""]?)(.*?)(?:['|""]?\))";
            Regex regex = new Regex(reg);
            MatchCollection matches = regex.Matches(styleContent);
            foreach (Match match in matches)
            {
               foreach (Group group in match.Groups)
               {
                   if (group.Value != match.Value)
                   {
                       var url = group.Value;
                       var qI = url.LastIndexOf('?');
                       if (qI > 0) url = url.Substring(0, qI);
                       var hI = url.LastIndexOf('#');
                       if (hI > 0) url = url.Substring(0, hI);
                       list.Add(url);
                   }
               }
            }
            return list.Distinct().ToList();
        }


        public static byte[] ZipPage(string html, IMapPathResolver mapPathResolver)
        {
            var parser = new AngleSharp.Parser.Html.HtmlParser();
            var doc = parser.Parse(html);
            var images = doc.Images;
            var styles = doc.GetElementsByTagName("link")
                .Where(l => l.Attributes["rel"].Value.Trim().ToLower() == "stylesheet");
            var scripts = doc.GetElementsByTagName("script");
            var serialAssets = new Dictionary<string, string>();
            serialAssets.AddSerializedAssets(images, "src");
            serialAssets.AddSerializedAssets(scripts, "src");
            var serialStyles = new Dictionary<string, string>();
            serialStyles.AddSerializedAssets(styles, "href");
            
            var newHtml = doc.ToHtml(new HtmlMarkupFormatter());
            var doneAssets = new List<string>();
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
                            doneAssets.Add("index.html");
                        }
    			    }
                    foreach (var serialStyle in serialStyles)
                    {
                        if (!doneAssets.Contains(serialStyle.Value))
                        {
                            var path = mapPathResolver.MapPath(serialStyle.Key);
                            var style = File.ReadAllText(path);
                            var urls = ExtaxtUrlsFromStyle(style);
                            foreach (var url in urls)
                            {
                                var localPath = ReturnLocalPath(url);
                                var suffix = localPath.Split('.').Last();
                                var newUrl = Guid.NewGuid().ToString().Replace("-", "") + "." + suffix;
                                style = style.Replace(url, newUrl);
                                if (!doneAssets.Contains(newUrl))
                                {
                                    zipArchive.AddBinaryAssetToArchive(newUrl, localPath, mapPathResolver);
                                    doneAssets.Add(newUrl);
                                }
                            }
                            var sentry = zipArchive.CreateEntry(serialStyle.Value, CompressionLevel.Fastest);
                            using (StreamWriter writer = new StreamWriter(sentry.Open()))
                            {
                                writer.Write(style);
                            }
                            doneAssets.Add(serialStyle.Value);
                        }
                    }
                    foreach (var serialAsset in serialAssets)
                    {
                        if (!doneAssets.Contains(serialAsset.Value))
                        {
                            zipArchive.AddBinaryAssetToArchive(serialAsset.Value, serialAsset.Key, mapPathResolver);
                            doneAssets.Add(serialAsset.Value);
                        }
                    }
    			}
                return ms.ToArray();
            }
        }

        private static void AddBinaryAssetToArchive(
            this ZipArchive zipArchive, 
            string serialAssetName, 
            string serialAssetPath,
            IMapPathResolver mapPathResolver)
        {
            var nentry = zipArchive.CreateEntry(serialAssetName, CompressionLevel.Fastest);
            using (var writer = new BinaryWriter(nentry.Open()))
            {
                var path = mapPathResolver.MapPath(serialAssetPath);
                var image = File.ReadAllBytes(path);
                writer.Write(image);
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
