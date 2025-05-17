using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Models;
using System.Collections.Generic;

namespace InventorySystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>()
                .Property(i => i.Price)
                .HasPrecision(18, 2); // 18 total digits with 2 decimal places

            // Check if any data exists before seeding
            // The data will only be seeded when the database is first created
            // This prevents duplicate key errors during migrations
        }
    }
}

