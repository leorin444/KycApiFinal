using Microsoft.EntityFrameworkCore;
using KycApi.Models;

namespace KycApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<KycApplication> KycApplications { get; set; } = null!;

        // ✅ Unified Users table for Admin + regular users
        public DbSet<User> Users { get; set; } = null!;

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed default admin
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "$2a$11$U3VcWQYVEkR92pT9XyM12ukf61eF8H/yx1Q4yx3wVFxT3vuWbDfQq", // password hash
                Role = "Admin",
                CreatedAt = new DateTime(2025, 12, 6, 12, 0, 0) // Static date
            });
        }
    }
}
