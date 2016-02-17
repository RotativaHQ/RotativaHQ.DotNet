﻿using AngleSharp.Dom;
using AngleSharp.Html;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static bool DetectBundle(string url)
        {
            var queryString = url.Split('?').LastOrDefault();
            var localPath = url;
            if (queryString != null && !url.Contains('.'))
            {
                return true;
            }
            return false;
        }

        private static string ReturnLocalPath(string url)
        {
            //if (DetectBundle(url))
            //    return string.Empty;

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
            foreach (var element in elements)
            {
                var src = element.Attributes[uriAttribute].Value;
                var localPath = ReturnLocalPath(src);
                if (localPath != string.Empty)
                {
                    var suffix = src.Split('.').Last();
                    if (suffix == localPath)
                    {
                        switch (element.TagName.ToLower())
                        {
                            case "link":
                                suffix = "css";
                                break;
                            case "script":
                                suffix = "js";
                                break;
                            default:
                                break;
                        };
                    }
                    var newSrc = Guid.NewGuid().ToString().Replace("-", "") + "." + suffix;
                    element.Attributes[uriAttribute].Value = newSrc;
                    serialAssets.Add(localPath, newSrc);
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


        public static byte[] ZipPage(string html, IMapPathResolver mapPathResolver, string webRoot)
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
                            try
                            {
                                writer.Write(newHtml);
                            }
                            catch (Exception ex)
                            {
                                EventLog.WriteEntry("demosite", ex.Message);
                                throw;
                            }
                            doneAssets.Add("index.html");
                        }
    			    }
                    foreach (var serialStyle in serialStyles)
                    {
                        if (!doneAssets.Contains(serialStyle.Value))
                        {
                            var style = GetStringAsset(serialStyle.Key, mapPathResolver, webRoot);
                            var urls = ExtaxtUrlsFromStyle(style);
                            foreach (var url in urls)
                            {
                                var localPath = ReturnLocalPath(url);
                                var suffix = localPath.Split('.').Last();
                                var newUrl = Guid.NewGuid().ToString().Replace("-", "") + "." + suffix;
                                style = style.Replace(url, newUrl);
                                if (!doneAssets.Contains(newUrl))
                                {
                                    zipArchive.AddBinaryAssetToArchive(newUrl, localPath, mapPathResolver, webRoot);
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
                            zipArchive.AddBinaryAssetToArchive(
                                serialAsset.Value, serialAsset.Key, mapPathResolver, webRoot);
                            doneAssets.Add(serialAsset.Value);
                        }
                    }
    			}
                return ms.ToArray();
            }
        }

        public static string GetStringAsset(string path, IMapPathResolver mapPathResolver, string webRoot)
        {
            if (DetectBundle(path))
            {
                using (var webClient = new WebClient())
                {
                    var style = webClient.DownloadString(webRoot + path);
                    return style;
                }
            }
            var localpath = mapPathResolver.MapPath(path);
            if (File.Exists(localpath))
            { 
                var style = File.ReadAllText(localpath);
                return style;
            }
            else
            {
                return string.Empty;
            }
        }

        public static byte[] GetBnaryAsset(string path, IMapPathResolver mapPathResolver, string webRoot)
        {
            if (DetectBundle(path))
            {
                using (var webClient = new WebClient())
                {
                    var asset = webClient.DownloadData(webRoot + path);
                    return asset;
                }
            }
            var localpath = mapPathResolver.MapPath(path);
            if (File.Exists(localpath))
            {
                var asset = File.ReadAllBytes(localpath);
                return asset;
            }
            else
            {
                throw new ArgumentException("no asset foun for "+ path);
            }
        }

        private static void AddBinaryAssetToArchive(
            this ZipArchive zipArchive, 
            string serialAssetName, 
            string serialAssetPath,
            IMapPathResolver mapPathResolver, string webRoot)
        {
            
            var nentry = zipArchive.CreateEntry(serialAssetName, CompressionLevel.Fastest);
            using (var writer = new BinaryWriter(nentry.Open()))
            {
                var asset = GetBnaryAsset(serialAssetPath, mapPathResolver, webRoot);
                writer.Write(asset);
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
