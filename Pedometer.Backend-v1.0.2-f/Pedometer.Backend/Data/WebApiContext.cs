using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Pedometer.Models;

namespace Pedometer.Data
{    // You can add profile data for users by adding more attributes to the ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 for details.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Please note that the authenticationType must match the corresponding item defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
    public class WebApiContext : IdentityDbContext<ApplicationUser>
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public WebApiContext() : base("name=WebApiContext")
        {
        }
        public static WebApiContext Create()
        {
            return new WebApiContext();
        }
        public System.Data.Entity.DbSet<StepInfo> StepInfos { get; set; }

        public System.Data.Entity.DbSet<UserInfo> UserInfos { get; set; }
    }
}
