﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RotativaHQ.Core
{
    public class Asset
    {
        public string Uri { get; set; }
        public string Suffix { get; set; }
        public string NewUri { get; set; }
    }

    public class AssetContent
    {
        public string Uri { get; set; }
        public string Suffix { get; set; }
        public string NewUri { get; set; }
        public byte[] Content { get; set; }
    }

    public class PackageBuilder
    {
        IMapPathResolver mapPathResolver;
        string htmlPage;
        string webRoot;
        //MemoryStream ms;
        //ZipArchive zipArchive;

        public PackageBuilder(IMapPathResolver mapPathResolver, string webRoot)
        {
            this.mapPathResolver = mapPathResolver;
            this.webRoot = webRoot;
            //ms = new MemoryStream();
            //zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true);
        }
        public List<Asset> GetHtmlAssets(string html)
        {
            var assets = new List<Asset>();
            var parser = new AngleSharp.Parser.Html.HtmlParser();
            var doc = parser.Parse(html.ToLowerInvariant());
            var images = doc.Images
                .Where(x => x.HasAttribute("src"));
            var styles = doc.GetElementsByTagName("link")
                .Where(l => l.Attributes["rel"].Value.Trim().ToLower() == "stylesheet")
                .Where(c => c.HasAttribute("href"));
            var scripts = doc.GetElementsByTagName("script")
                .Where(x => x.HasAttribute("src"));

            foreach (var image in images)
            {
                var src = image.Attributes["src"].Value;
                if (IsLocalPath(src) && !assets.Any(a => a.Uri == src))
                {
                    var suffix = src.Split('.').Last().Split('?').First().Split('#').First();
                    var asset = new Asset
                    {
                        Uri = src,
                        Suffix = suffix,
                        NewUri = Guid.NewGuid().ToString().Replace("-", "")
                    };
                    assets.Add(asset);
                }
            }
            foreach (var css in styles)
            {
                var src = css.Attributes["href"].Value;
                if (IsLocalPath(src) && !assets.Any(a => a.Uri == src))
                {
                    var asset = new Asset
                    {
                        Uri = src,
                        Suffix = "css",
                        NewUri = Guid.NewGuid().ToString().Replace("-", "")
                    };
                    assets.Add(asset);
                }
            }
            foreach (var script in scripts)
            {
                var src = script.Attributes["src"].Value;
                if (IsLocalPath(src) && !assets.Any(a => a.Uri == src))
                {
                    var suffix = src.Split('.').Last().Split('?').First().Split('#').First();
                    if (suffix == src.Split('?').First())
                        suffix = "js";
                    var asset = new Asset
                    {
                        Uri = src,
                        Suffix = suffix,
                        NewUri = Guid.NewGuid().ToString().Replace("-", "")
                    };
                    assets.Add(asset);
                }
            }

            return assets;
        }

        public List<Asset> GetCssAssets(string css)
        {
            var assets = new List<Asset>();
            var urls = Zipper.ExtaxtUrlsFromStyle(css);
            foreach (var src in urls.Where(s => IsLocalPath(s)))
            {
                var suffix = src.Split('.').Last().Split('?').First().Split('#').First();
                var asset = new Asset
                {
                    Uri = src,
                    Suffix = suffix,
                    NewUri = Guid.NewGuid().ToString().Replace("-", "")
                };
                assets.Add(asset);
            }

            return assets;
        }

        public static bool IsLocalPath(string url)
        {
            if (!url.ToLower().StartsWith("http://") && !url.StartsWith("//"))
            {
                return true;
            }
            Uri uri = new Uri(url);
            var hostname = uri.Host;
            var isLocaHost = IsLocalhost(hostname);
            return isLocaHost;
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

        public List<AssetContent> GetAssetsContents(string html, string pagePath, string htmlName)
        {
            var assetsContents = new List<AssetContent>();
            var htmlAssets = GetHtmlAssets(html);
            foreach (var asset in htmlAssets.Where(a => a.Suffix != "css"))
            {
                try
                {
                    var assetContent = GetBinaryAsset(
                        asset.Uri, this.mapPathResolver, this.webRoot, pagePath);
                    if (!assetsContents.Any(a => a.Uri == asset.Uri))
                    {
                        assetsContents.Add(new AssetContent
                        {
                            Uri = asset.Uri,
                            NewUri = asset.NewUri,
                            Suffix = asset.Suffix,
                            Content = assetContent
                        });
                        // TODO: use regex to avoid replace uri that is not link but text
                        
                    }
                }
                catch (Exception ex)
                {
                    // TODO: trace somewhere
                }
            }

            // finally add index html
            foreach (var assetContent in assetsContents)
            {
                html.Replace(assetContent.Uri, assetContent.NewUri + "." + assetContent.Suffix);
            }
            var htmlContent = Encoding.UTF8.GetBytes(html);
            assetsContents.Add(new AssetContent
            {   
                Uri = pagePath,
                NewUri = htmlName,
                Suffix = "html",
                Content = htmlContent
            });
            return assetsContents;
        }

        public static byte[] GetBinaryAsset(string path, IMapPathResolver mapPathResolver, string webRoot, string pagePath)
        {
            byte[] content = null;
            try
            {
                var localpath = mapPathResolver.MapPath(pagePath, path);
                if (File.Exists(localpath))
                {
                    content = File.ReadAllBytes(localpath);
                }
            }
            catch (Exception ex)
            {
                // TODO: trace somewhere
            }
            if (content == null)
            {
                using (var webClient = new WebClient())
                {
                    try
                    {
                        content = webClient.DownloadData(webRoot + path);
                    }
                    catch (Exception ex)
                    {
                        // TODO: trace somewhere
                    }
                }
            }
            return content;
        }

    }
}