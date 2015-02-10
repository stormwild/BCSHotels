using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BCSHotels.Startup))]
namespace BCSHotels
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
