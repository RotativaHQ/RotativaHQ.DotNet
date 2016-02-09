using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotativaHQ.Core
{
    public interface IMapPathResolver
    {
        string MapPath(string virtualPath);
    }
}
