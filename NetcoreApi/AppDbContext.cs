using Microsoft.EntityFrameworkCore;
using NetcoreApi.Models;
using NetcoreApi.Models.Order;

namespace NetcoreApi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<FileMetadata> FileMetadata { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // Seed Data
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.Parse("01E09B5D-26A2-4B9C-8B62-181DA1ACEB04"),
                    Name = "John Doe",
                    Email = "john@example.com",
                    Password = "password123",
                    CreatedAt = DateTime.Parse("2026-01-25 14:16:57.985830", null, System.Globalization.DateTimeStyles.AssumeUniversal),
                    IsActive = true
                },
                new User
                {
                    Id = Guid.Parse("CB1A41F2-027E-4432-AD0C-AD80CB3D112B"),
                    Name = "Jane Smith",
                    Email = "jane@example.com",
                    Password = "password123",
                    CreatedAt = DateTime.Parse("2026-01-25 14:17:33.279110", null, System.Globalization.DateTimeStyles.AssumeUniversal),
                    IsActive = true
                },
                new User
                {
                    Id = Guid.Parse("F5E0BF5C-DE26-4B69-9795-1C8CAF1E5F37"),
                    Name = "Bob Johnson",
                    Email = "bob@example.com",
                    Password = "password123",
                    CreatedAt = DateTime.Parse("2026-01-25 14:17:41.352879", null, System.Globalization.DateTimeStyles.AssumeUniversal),
                    IsActive = true
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = Guid.Parse("1E09B5D0-26A2-4B9C-8B62-181DA1ACEB01"),
                    Name = "Laptop",
                    Description = "High-performance laptop",
                    Price = 999.99m,
                    Stock = 10,
                    CreatedAt = DateTime.Parse("2026-01-25 14:18:06.827888", null, System.Globalization.DateTimeStyles.AssumeUniversal),
                },
                new Product
                {
                    Id = Guid.Parse("2E09B5D0-26A2-4B9C-8B62-181DA1ACEB02"),
                    Name = "Mouse",
                    Description = "Wireless mouse",
                    Price = 29.99m,
                    Stock = 50,
                    CreatedAt = DateTime.Parse("2026-01-25 14:18:16.053820", null, System.Globalization.DateTimeStyles.AssumeUniversal)
                },
                new Product
                {
                    Id = Guid.Parse("3E09B5D0-26A2-4B9C-8B62-181DA1ACEB03"),
                    Name = "Keyboard",
                    Description = "Mechanical keyboard",
                    Price = 89.99m,
                    Stock = 30,
                    CreatedAt = DateTime.Parse("2026-01-25 14:18:34.454793", null, System.Globalization.DateTimeStyles.AssumeUniversal)
                }
            );
        }
    }
}
