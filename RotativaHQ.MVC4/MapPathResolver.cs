using RotativaHQ.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RotativaHQ.MVC4
{
    public class MapPathResolver : IMapPathResolver
    {
        public string MapPath(string startPath, string virtualPath)
        {
            string localPath = "";
            if (virtualPath.StartsWith("/"))
            {
                localPath = HttpContext.Current.Server.MapPath(virtualPath);
            }
            else
            {
                // not sure this really works, 
                // let's say we support only absolute local uri,
                // so only the one starting with /
                startPath = startPath.Remove(startPath.LastIndexOf('/') + 1);
                localPath = HttpContext.Current.Server.MapPath(startPath + virtualPath);
            }
            return localPath;
        }
    }
}
