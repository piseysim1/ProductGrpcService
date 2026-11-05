using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static ProductGrpcService.Domain.SampleDb;
using static ProductGrpcService.Helper.ConfigKeys;

namespace ProductGrpcService
{
    public class SampleDbContext(IConfiguration configuration) : DbContext
    {
        private readonly IConfiguration _configuration = configuration;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(_configuration.GetConnectionString(DatabaseConfigKeys.ConnectionString),
                x => x.MigrationsHistoryTable(DatabaseConfigKeys.SampleVersion));
            base.OnConfiguring(options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.InitSomeData(_configuration);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName($"sample_{entityType.GetTableName()}");
            }
        }

       
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
    }

    public static class DbContextSeedData
    {
        public static void InitSomeData(this ModelBuilder mb, IConfiguration configuration)
        {
           
            mb.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Accessories", Description = "Computer peripherals and accessories" },
                new Category { Id = 2, Name = "Keyboards", Description = "Mechanical and membrane keyboards" }
            );

            // 2. Seed Inventories
            mb.Entity<Inventory>().HasData(
                new Inventory { Id = 1, ProductId = 1, StockQuantity = 500, WarehouseLocation = "Main Warehouse" }
            );

            // 3. Seed Products
            mb.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Wireless Gaming Mouse",
                    Description = "High precision RGB mouse",
                    Price = 59,
                    CategoryId = 1,
                    InventoryId = 1
                },
                new Product
                {
                    Id = 2,
                    Name = "Mechanical Keyboard",
                    Description = "Blue switch tactile keyboard",
                    Price = 89,
                    CategoryId = 2,
                    InventoryId = 1
                },
                new Product
                {
                    Id = 3,
                    Name = "UltraWide 4K Monitor",
                    Description = "34-inch curved display",
                    Price = 399,
                    CategoryId = 1,
                    InventoryId = 1
                },
                new Product
                {
                    Id = 4,
                    Name = "Noise-Cancelling Headset",
                    Description = "Over-ear wireless audio",
                    Price = 149,
                    CategoryId = 1,
                    InventoryId = 1
                },
                new Product
                {
                    Id = 5,
                    Name = "USB-C Hub Multiport",
                    Description = "7-in-1 adapter dongle",
                    Price = 29,
                    CategoryId = 1,
                    InventoryId = 1
                }
            );
        }
    }
}