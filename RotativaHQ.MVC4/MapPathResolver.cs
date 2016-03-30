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
                startPath = startPath.Remove(startPath.LastIndexOf('/') + 1);
                try
                {

                    localPath = HttpContext.Current.Server.MapPath(startPath + virtualPath);
                }
                catch (HttpException hex)
                {
                    var rootLocalPath = "/" + virtualPath.Replace("../", "");
                    localPath = HttpContext.Current.Server.MapPath(rootLocalPath);
                }
                catch (Exception ex)
                {
                    localPath = virtualPath;
                }
            }
            return localPath;
        }
    }
}
