using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RotativaHQ.MVC4
{
    public sealed class GzipContent : HttpContent
    {
        private readonly HttpContent content;

        public GzipContent(HttpContent content)
        {
            this.content = content;

            // Keep the original content's headers ...
            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // ... and let the server know we've Gzip-compressed the body of this request.
            Headers.ContentEncoding.Add("gzip");
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            // Open a GZipStream that writes to the specified output stream.
            using (GZipStream gzip = new GZipStream(stream, CompressionMode.Compress, true))
            {
                // Copy all the input content to the GZip stream.
                await content.CopyToAsync(gzip);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
