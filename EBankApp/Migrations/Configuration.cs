namespace EBankApp.Migrations
{
    using EBankApp.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<EBankApp.DatabaseContext.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(EBankApp.DatabaseContext.AppDbContext context)
        {

            try
            {
                context.Users.AddOrUpdate(
                new User { Id = 1, FirstName = "Adminmmmm", LastName = "EBank", Password = "Adminmmm", PIN = "888605", UserName = "Adminmmm" }
            );

                context.Roles.AddOrUpdate(
                   new Role { Id = 1, Name = "Admin", CreatedBy = 1 },
                   new Role { Id = 2, Name = "User", CreatedBy = 1 }
                );

                context.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }
                throw new Exception(sb.ToString());
            }

        }
    }
}
