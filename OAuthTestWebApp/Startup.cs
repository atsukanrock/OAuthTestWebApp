using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OAuthTestWebApp.Startup))]
namespace OAuthTestWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
