﻿using RotativaHQ.Core;
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
                localPath = HttpContext.Current.Server.MapPath(startPath + virtualPath);
            }
            return localPath;
        }
    }
}
