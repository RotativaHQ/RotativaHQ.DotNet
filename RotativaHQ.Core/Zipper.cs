using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotativaHQ.Core
{
    public class Zipper
    {
        public static byte[] ZipPage(string html)
        {
            //return new byte[] { };
            using (var ms = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
    			{
    			    //foreach (var attachment in attachmentFiles)
    			    {
    			        var entry = zipArchive.CreateEntry("index.html", CompressionLevel.Fastest);
    			        
                        using (StreamWriter writer = new StreamWriter(entry.Open()))
                        {
                            writer.Write(html);
                        }
    			    }
    			}
                return ms.ToArray();
            }
            
        }
    }
}
