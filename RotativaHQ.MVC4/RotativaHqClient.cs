using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RotativaHQ.MVC4
{
    public class RotativaHqClient
    {
        string apiKey;

        public RotativaHqClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public byte[] Call(string switches, string html)
        {
            var httpClient = new HttpClient();
            //var html = File.ReadAllText(HostingEnvironment.ApplicationPhysicalPath + "/test.html");
            using (
                var request = CreateRequest("/", "application/json", HttpMethod.Post))
            {
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("html", html));
                postData.Add(new KeyValuePair<string, string>("switches", switches));
                postData.Add(new KeyValuePair<string, string>("fileName", ""));

                HttpContent content = new FormUrlEncodedContent(postData);
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
                    var wc = new WebClient();
                    var pdf = wc.DownloadData(pdfUrl); //Convert.FromBase64String(pdfString);
                    return pdf;
                }
            }
        }

        public string GetPdfUrl(string switches, string html, string fileName = "")
        {
            var httpClient = new HttpClient();
            //var html = File.ReadAllText(HostingEnvironment.ApplicationPhysicalPath + "/test.html");
            using (
                var request = CreateRequest("/", "application/json", HttpMethod.Post))
            {
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("fileName", fileName ?? ""));
                postData.Add(new KeyValuePair<string, string>("html", html));
                postData.Add(new KeyValuePair<string, string>("switches", switches));

                HttpContent content = new FormUrlEncodedContent(postData);
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
            //var apiUrl = "http://localhost:1282";
            //var apiUrl = "http://localhost:53460";
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(apiUrl + url)
            };
            //HttpContext.Current = new HttpContext(new HttpRequest(null, apiUrl + url, null), new HttpResponse(null));
            //HttpContext.Current.User = new ClaimsPrincipal(new ClaimsIdentity());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mthv));
            request.Method = method;
            Debug.WriteLine("Method: " + request.Method);
            Debug.WriteLine("Url: " + request.RequestUri);
            Debug.WriteLine("Headers: ");
            Debug.WriteLine("\t" + request.Headers.ToString());
            return request;
        }
    }
}
