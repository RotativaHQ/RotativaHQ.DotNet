using RotativaHQ.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RotativaHQ.MVC5
{
    public class MapPathResolver : IMapPathResolver
    {
        public string MapPath(string virtualPath)
        {
            return HttpContext.Current.Server.MapPath(virtualPath);
        }
    }
}
