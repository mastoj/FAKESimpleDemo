using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FAKESimple.Web.Startup))]
namespace FAKESimple.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
