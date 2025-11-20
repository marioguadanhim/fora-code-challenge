using Fora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Fora.Infra.Data.EntityMapping;

namespace Fora.Infra.Data.Context
{
    public class ForaContext : DbContext
    {
        private IConfiguration _configuration;

        public DbSet<Company> Company { get; set; }
        public DbSet<CompanyNetIncomeLoss> CompanyNetIncomeLoss { get; set; }
        public DbSet<WebUser> WebUser { get; set; }

        public ForaContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = _configuration.GetSection("ForaConnectionString").Value;
            optionsBuilder.UseSqlServer(connection);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CompanyMapping());
            modelBuilder.ApplyConfiguration(new CompanyNetIncomeLossMapping());
            modelBuilder.ApplyConfiguration(new WebUserMapping());


            modelBuilder.Entity<WebUser>().HasData(
              new WebUser
              {
                  UserName = "guest",
                  Password = "h1eTenbV4CdRr5fYIYmiq/1mg+6i3WL7ELVS0WsLmB8=",
                  Role = WebUserRoleEnum.Guest
              }
            );
        }

    }
}
