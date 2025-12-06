using Microsoft.EntityFrameworkCore;
using KycApi.Models;
using KycApi.Data;


namespace KycApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<KycApplication> KycApplications { get; set; } = null!;

        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Admin>().HasData(new Admin
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "$2a$11$U3VcWQYVEkR92pT9XyM12ukf61eF8H/yx1Q4yx3wVFxT3vuWbDfQq",
                Role = "Admin"
            });
        }



    }
}
