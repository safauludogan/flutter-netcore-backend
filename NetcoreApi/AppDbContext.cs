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
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "John Doe",
                    Email = "john@example.com",
                    Password = "password123",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Jane Smith",
                    Email = "jane@example.com",
                    Password = "password123",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Bob Johnson",
                    Email = "bob@example.com",
                    Password = "password123",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Laptop",
                    Description = "High-performance laptop",
                    Price = 999.99m,
                    Stock = 10,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Mouse",
                    Description = "Wireless mouse",
                    Price = 29.99m,
                    Stock = 50,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Keyboard",
                    Description = "Mechanical keyboard",
                    Price = 89.99m,
                    Stock = 30,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
