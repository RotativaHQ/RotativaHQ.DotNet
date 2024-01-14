using Microsoft.Owin;
using Owin;
using System.Net;

[assembly: OwinStartupAttribute(typeof(RotativaHq.MVC5.DemoSite.Startup))]
namespace RotativaHq.MVC5.DemoSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ConfigureAuth(app);
        }
    }
}
