using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RotativaHQ.MVC5
{
    public sealed class GzipCompressingHandler : DelegatingHandler
    {
        public GzipCompressingHandler(HttpMessageHandler innerHandler)
        {
            if (null == innerHandler)
            {
                throw new ArgumentNullException("innerHandler");
            }

            InnerHandler = innerHandler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpContent content = request.Content;

            if (request.Method == HttpMethod.Post)
            {
                // Wrap the original HttpContent in our custom GzipContent class.
                // If you want to compress only certain content, make the decision here!
                request.Content = new GzipContent(request.Content);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
