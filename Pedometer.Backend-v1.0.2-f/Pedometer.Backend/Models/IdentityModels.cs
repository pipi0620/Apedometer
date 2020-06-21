using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Admin.Models
{
    // 可以通过将更多属性添加到 ApplicationUser 类来为用户添加配置文件数据，请访问 https://go.microsoft.com/fwlink/?LinkID=317594 了解详细信息。
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // 请注意，authenticationType 必须与 CookieAuthenticationOptions.AuthenticationType 中定义的相应项匹配
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在此处添加自定义用户声明
            return userIdentity;
        }
    }
    public class UserInfo
    {
        public String Id { get; set; }
        public String UserName { get; set; }
        public String Password { get; set; }
        public String Phone { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public string DeviceToken { get; set; }
    }
    public class StepInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //设置自增
        public int Id { get; set; }
        public String UserId { get; set; }
        public long TimeStamp { get; set; }
        public int TodaySteps { get; set; }
        public float Distance { get; set; }
        public float Calories { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }


        public System.Data.Entity.DbSet<Admin.Models.StepInfo> StepInfos { get; set; }

        public System.Data.Entity.DbSet<Admin.Models.UserInfo> UserInfos { get; set; }
    }
}