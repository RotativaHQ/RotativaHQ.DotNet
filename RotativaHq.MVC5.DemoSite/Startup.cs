using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RotativaHq.MVC5.DemoSite.Startup))]
namespace RotativaHq.MVC5.DemoSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
