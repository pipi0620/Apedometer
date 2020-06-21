using System.Data.Entity.Migrations;
using Pedometer.Data;

namespace Pedometer.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<WebApiContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(WebApiContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
