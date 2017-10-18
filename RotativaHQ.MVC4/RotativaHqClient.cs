using Newtonsoft.Json.Linq;
using ProtoBuf;
using RotativaHQ.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RotativaHQ.MVC4
{
    public class RotativaHqClient
    {
        string apiKey;

        public RotativaHqClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public string GetPdfUrl(string switches, string html, string fileName = "", string header = "", string footer = "", string contentDisposition = "")
        {
            var context = HttpContext.Current;
            var webRoot = string.Format("{0}://{1}{2}",
                context.Request.Url.Scheme,
                context.Request.Url.Host,
                context.Request.Url.Port == 80
                  ? string.Empty : ":" + context.Request.Url.Port);
            webRoot = webRoot.TrimEnd('/');
            var requestPath = context.Request.Path;
            var packageBuilder = new PackageBuilder(new MapPathResolver(), webRoot);
            packageBuilder.AddHtmlToPackage(html, requestPath, "index");
            if (!string.IsNullOrEmpty(header))
            {
                packageBuilder.AddHtmlToPackage(header, requestPath, "header");
            }
            if (!string.IsNullOrEmpty(footer))
            {
                packageBuilder.AddHtmlToPackage(footer, requestPath, "footer");
            }
            var assets = packageBuilder.AssetsContents
                .Select(a => new KeyValuePair<string, byte[]>(
                    a.NewUri + "." + a.Suffix, a.Content))
                .ToDictionary(x => x.Key, x => x.Value);
            var payload = new PdfRequestPayloadV2
            {
                Id = Guid.NewGuid(),
                Filename = fileName,
                Switches = switches,
                HtmlAssets = assets,
                ContentDisposition = contentDisposition
            };
            string gzipIt = ConfigurationManager.AppSettings["RotativaGZip"];
            if (HttpContext.Current != null && HttpContext.Current.Request.IsLocal && gzipIt == null)
            {
                gzipIt = "1";
            }
            if (gzipIt == "1")
            {
                var httpClient = new HttpClient(new GzipCompressingHandler(new HttpClientHandler()));
                using (
                    var request = CreateRequest("/v2", "application/json", HttpMethod.Post))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var sw = new StreamWriter(ms);//, new UnicodeEncoding());
                        Serializer.Serialize(ms, payload);
                        ms.Position = 0;
                        HttpContent content = new StreamContent(ms);
                        request.Content = content; // new GzipContent(content);
                        using (
                            HttpResponseMessage response =
                                httpClient.SendAsync(request, new CancellationTokenSource().Token).Result)
                        {
                            var httpResponseMessage = response;
                            var result = response.Content.ReadAsStringAsync();
                            var jsonReponse = JObject.Parse(result.Result);
                            if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                var error = jsonReponse["error"].Value<string>();
                                throw new UnauthorizedAccessException(error);
                            }
                            var pdfUrl = jsonReponse["pdfUrl"].Value<string>(); // 
                            return pdfUrl;
                        }
                    }
                }
            }
            else
            {
                var httpClient = new HttpClient();
                using (
                    var request = CreateRequest("/v2", "application/json", HttpMethod.Post))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var sw = new StreamWriter(ms, new UnicodeEncoding());
                        Serializer.Serialize(ms, payload);
                        ms.Position = 0;
                        HttpContent content = new StreamContent(ms);
                        request.Content = content;

                        using (
                            HttpResponseMessage response =
                                httpClient.SendAsync(request, new CancellationTokenSource().Token).Result)
                        {
                            var httpResponseMessage = response;
                            var result = response.Content.ReadAsStringAsync();
                            var jsonReponse = JObject.Parse(result.Result);
                            if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                var error = jsonReponse["error"].Value<string>();
                                throw new UnauthorizedAccessException(error);
                            }
                            var pdfUrl = jsonReponse["pdfUrl"].Value<string>(); // 
                            return pdfUrl;
                        }
                    }
                }

            }
        }

        /// <summary>
        /// This method is taken from Filip W in a blog post located at: http://www.strathweb.com/2012/06/asp-net-web-api-integration-testing-with-in-memory-hosting/
        /// </summary>
        /// <param name="url"></param>
        /// <param name="mthv"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        protected HttpRequestMessage CreateRequest(string url, string mthv, HttpMethod method)
        {
            var request = CreateRawRequest(url, mthv, method);
            request.Headers.Add("X-ApiKey", apiKey);
            return request;
        }

        protected HttpRequestMessage CreateRawRequest(string url, string mthv, HttpMethod method)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains("RotativaUrl"))
            {
                throw new ConfigurationErrorsException("RotativaUrl AppSetting not found");
            }
            var apiUrl = ConfigurationManager.AppSettings["RotativaUrl"].ToString();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(apiUrl + url)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mthv));
            request.Method = method;
            Debug.WriteLine("Method: " + request.Method);
            Debug.WriteLine("URL: " + request.RequestUri);
            Debug.WriteLine("Headers: ");
            Debug.WriteLine("\t" + request.Headers.ToString());
            return request;
        }
    }
}
