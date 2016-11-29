using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HotelMVC.Startup))]
namespace HotelMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
