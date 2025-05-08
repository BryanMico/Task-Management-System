namespace TMS.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using TMS.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<TMS.Models.TMS_DB>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(TMS.Models.TMS_DB context)
        {
            context.Roles.AddOrUpdate(r => r.Id,
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Client" }
            );

            if (!context.Users.Any(u => u.Email == "admin@iraya.com"))
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword("adminpass20", 12);

                context.Users.Add(new User
                {
                    FullName = "Iraya Admin",
                    Email = "admin@iraya.com",
                    PasswordHash = hashedPassword,
                    RoleId = 1,
                    IsActive = true,
                    CreatedByAdminId = null,
                    IsNewUser = true
                });
            }

            context.SaveChanges(); 
        }
    }
}
