using Microsoft.Owin;
using Owin;
using Pedometer;

[assembly: OwinStartup(typeof(Startup))]
namespace Pedometer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
