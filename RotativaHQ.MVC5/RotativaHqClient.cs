﻿using Newtonsoft.Json.Linq;
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

namespace RotativaHQ.MVC5
{
    public class RotativaHqClient
    {
        string apiKey;

        public RotativaHqClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public string GetPdfUrl(string switches, string html, string fileName = "", string header = "", string footer = "")
        {
            var httpClient = new HttpClient();
            using (
                var request = CreateRequest("/", "application/json", HttpMethod.Post))
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
                var zippedHtml = packageBuilder.GetPackage();
                var payload = new PdfRequestPayload{
                    Id = Guid.NewGuid(),
                    Filename = fileName,
                    Switches = switches,
                    ZippedHtmlPage = zippedHtml
                };
                
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
